using CricketAPI.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace CricketAPI.Services
{
    public interface ICricketService
    {
        Task<MatchListResponse> GetLiveMatchesAsync();
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
            // Try cache first
            try
            {
                if (_redis != null)
                {
                    var db = _redis.GetDatabase();
                    var cached = await db.StringGetAsync(CACHE_KEY);
                    if (cached.HasValue)
                    {
                        _logger.LogInformation("Returning cached match data");
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
                _logger.LogWarning(ex, "Redis cache read failed, falling back to API");
            }

            // Fetch from API
            try
            {
                var response = await _httpClient.GetAsync($"{BASE_URL}/currentMatches?apikey={API_KEY}&offset=0");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("CricAPI returned status: {Status}", response.StatusCode);
                    return new MatchListResponse { Success = false, Message = "Failed to fetch from cricket API" };
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<CricApiResponse>(json, JsonOptions);

                if (apiResponse?.Data == null)
                    return new MatchListResponse { Success = false, Message = "No data from cricket API" };

                var matches = apiResponse.Data.Select(m =>
                {
                    var scoreText = BuildScoreText(m.Score);
                    return new CricketMatch
                    {
                        MatchId = m.Id,
                        Name = m.Name,
                        Team1 = m.Teams?.ElementAtOrDefault(0) ?? "TBD",
                        Team2 = m.Teams?.ElementAtOrDefault(1) ?? "TBD",
                        Team1Img = m.TeamInfo?.ElementAtOrDefault(0)?.Img,
                        Team2Img = m.TeamInfo?.ElementAtOrDefault(1)?.Img,
                        Status = m.Status ?? "Unknown",
                        Score = scoreText,
                        MatchType = m.MatchType?.ToUpper() ?? "N/A",
                        StartTime = DateTime.TryParse(m.DateTimeGMT, out var dt) ? dt : null,
                        Venue = m.Venue,
                        MatchStarted = m.MatchStarted,
                        MatchEnded = m.MatchEnded
                    };
                }).ToList();

                var result = new MatchListResponse
                {
                    Matches = matches,
                    Success = true,
                    Message = $"Retrieved {matches.Count} matches (live from API)"
                };

                // Store in cache
                try
                {
                    if (_redis != null)
                    {
                        var db = _redis.GetDatabase();
                        var cacheJson = JsonSerializer.Serialize(result, JsonOptions);
                        await db.StringSetAsync(CACHE_KEY, cacheJson, TimeSpan.FromMinutes(CACHE_MINUTES));
                        _logger.LogInformation("Match data cached for {Minutes} minutes", CACHE_MINUTES);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Redis cache write failed");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching cricket matches");
                return new MatchListResponse { Success = false, Message = $"Error: {ex.Message}" };
            }
        }

        private static string BuildScoreText(List<CricApiScore>? scores)
        {
            if (scores == null || scores.Count == 0)
                return "Score not available";

            return string.Join(" | ", scores.Select(s => $"{s.Inning}: {s.R}/{s.W} ({s.O} ov)"));
        }
    }
}