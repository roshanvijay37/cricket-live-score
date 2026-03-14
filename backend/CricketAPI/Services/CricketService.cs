using CricketAPI.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace CricketAPI.Services
{
    public interface ICricketService
    {
        Task<MatchListResponse> GetLiveMatchesAsync();
        Task<MatchDetailResponse> GetMatchDetailAsync(string matchId);
    }

    public class CricketService : ICricketService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CricketService> _logger;
        private readonly IConnectionMultiplexer? _redis;
        private const string API_KEY = "30095090-1180-443f-a843-c7ba083f86a6";
        private const string BASE_URL = "https://api.cricapi.com/v1";
        private const string CACHE_KEY = "cricket:matches";
        private const int CACHE_MINUTES = 5;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public CricketService(HttpClient httpClient, ILogger<CricketService> logger, IConnectionMultiplexer? redis = null)
        {
            _httpClient = httpClient;
            _logger = logger;
            _redis = redis;
        }

        public async Task<MatchListResponse> GetLiveMatchesAsync()
        {
            try
            {
                if (_redis != null)
                {
                    var db = _redis.GetDatabase();
                    var cached = await db.StringGetAsync(CACHE_KEY);
                    if (cached.HasValue)
                    {
                        var cachedResponse = JsonSerializer.Deserialize<MatchListResponse>(cached!, JsonOptions);
                        if (cachedResponse != null)
                        {
                            cachedResponse.Message = "From cache (refreshes every 5 min)";
                            return cachedResponse;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis cache read failed");
            }

            try
            {
                var response = await _httpClient.GetAsync($"{BASE_URL}/currentMatches?apikey={API_KEY}&offset=0");
                if (!response.IsSuccessStatusCode)
                    return new MatchListResponse { Success = false, Message = "Failed to fetch from cricket API" };

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<CricApiResponse>(json, JsonOptions);
                if (apiResponse?.Data == null)
                    return new MatchListResponse { Success = false, Message = "No data from cricket API" };

                var matches = apiResponse.Data.Select(MapToMatch).ToList();
                var result = new MatchListResponse
                {
                    Matches = matches,
                    Success = true,
                    Message = $"Retrieved {matches.Count} matches (live from API)"
                };

                try
                {
                    if (_redis != null)
                    {
                        var db = _redis.GetDatabase();
                        await db.StringSetAsync(CACHE_KEY, JsonSerializer.Serialize(result, JsonOptions), TimeSpan.FromMinutes(CACHE_MINUTES));
                    }
                }
                catch (Exception ex) { _logger.LogWarning(ex, "Redis cache write failed"); }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching cricket matches");
                return new MatchListResponse { Success = false, Message = $"Error: {ex.Message}" };
            }
        }

        public async Task<MatchDetailResponse> GetMatchDetailAsync(string matchId)
        {
            var cacheKey = $"cricket:match:v2:{matchId}";
            try
            {
                if (_redis != null)
                {
                    var db = _redis.GetDatabase();
                    var cached = await db.StringGetAsync(cacheKey);
                    if (cached.HasValue)
                    {
                        var cachedResponse = JsonSerializer.Deserialize<MatchDetailResponse>(cached!, JsonOptions);
                        if (cachedResponse != null)
                        {
                            cachedResponse.Message = "From cache";
                            return cachedResponse;
                        }
                    }
                }
            }
            catch (Exception ex) { _logger.LogWarning(ex, "Redis cache read failed"); }

            try
            {
                // Fetch match info
                var response = await _httpClient.GetAsync($"{BASE_URL}/match_info?apikey={API_KEY}&id={matchId}");
                if (!response.IsSuccessStatusCode)
                    return new MatchDetailResponse { Success = false, Message = "Failed to fetch match details" };

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<CricApiSingleResponse>(json, JsonOptions);
                if (apiResponse?.Data == null)
                    return new MatchDetailResponse { Success = false, Message = "Match not found" };

                var match = MapToMatch(apiResponse.Data);

                // Fetch squad if available
                if (apiResponse.Data.HasSquad)
                {
                    try
                    {
                        var squadResponse = await _httpClient.GetAsync($"{BASE_URL}/match_squad?apikey={API_KEY}&id={matchId}");
                        if (squadResponse.IsSuccessStatusCode)
                        {
                            var squadJson = await squadResponse.Content.ReadAsStringAsync();
                            var squadData = JsonSerializer.Deserialize<CricApiSquadResponse>(squadJson, JsonOptions);
                            if (squadData?.Data != null && squadData.Data.Count > 0)
                            {
                                match.Squads = squadData.Data.Select(s => new TeamSquad
                                {
                                    TeamName = s.TeamName,
                                    Shortname = s.Shortname,
                                    Img = s.Img,
                                    Players = s.Players?.Select(p => new Player
                                    {
                                        Name = p.Name,
                                        Role = p.Role,
                                        BattingStyle = p.BattingStyle,
                                        BowlingStyle = p.BowlingStyle,
                                        PlayerImg = p.PlayerImg
                                    }).ToList()
                                }).ToList();
                            }
                        }
                    }
                    catch (Exception ex) { _logger.LogWarning(ex, "Failed to fetch squad"); }
                }

                var result = new MatchDetailResponse
                {
                    Match = match,
                    Success = true,
                    Message = "Match details retrieved"
                };

                try
                {
                    if (_redis != null)
                    {
                        var db = _redis.GetDatabase();
                        await db.StringSetAsync(cacheKey, JsonSerializer.Serialize(result, JsonOptions), TimeSpan.FromMinutes(3));
                    }
                }
                catch (Exception ex) { _logger.LogWarning(ex, "Redis cache write failed"); }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching match detail");
                return new MatchDetailResponse { Success = false, Message = $"Error: {ex.Message}" };
            }
        }

        private static CricketMatch MapToMatch(CricApiMatch m)
        {
            return new CricketMatch
            {
                MatchId = m.Id,
                Name = m.Name,
                Team1 = m.Teams?.ElementAtOrDefault(0) ?? "TBD",
                Team2 = m.Teams?.ElementAtOrDefault(1) ?? "TBD",
                Team1Img = m.TeamInfo?.ElementAtOrDefault(0)?.Img,
                Team2Img = m.TeamInfo?.ElementAtOrDefault(1)?.Img,
                Status = m.Status ?? "Unknown",
                Score = BuildScoreText(m.Score),
                MatchType = m.MatchType?.ToUpper() ?? "N/A",
                StartTime = DateTime.TryParse(m.DateTimeGMT, out var dt) ? dt : null,
                Venue = m.Venue,
                MatchStarted = m.MatchStarted,
                MatchEnded = m.MatchEnded,
                TossWinner = m.TossWinner,
                TossChoice = m.TossChoice,
                ScoreDetails = m.Score?.Select(s => new ScoreDetail
                {
                    Inning = s.Inning,
                    Runs = s.R,
                    Wickets = s.W,
                    Overs = s.O
                }).ToList()
            };
        }

        private static string BuildScoreText(List<CricApiScore>? scores)
        {
            if (scores == null || scores.Count == 0)
                return "Score not available";
            return string.Join(" | ", scores.Select(s => $"{s.Inning}: {s.R}/{s.W} ({s.O} ov)"));
        }
    }
}