using CricketAPI.Models;
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
        private const string API_KEY = "30095090-1180-443f-a843-c7ba083f86a6";
        private const string BASE_URL = "https://api.cricapi.com/v1";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public CricketService(HttpClient httpClient, ILogger<CricketService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<MatchListResponse> GetLiveMatchesAsync()
        {
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
                {
                    return new MatchListResponse { Success = false, Message = "No data from cricket API" };
                }

                var matches = apiResponse.Data.Select(m =>
                {
                    var scoreText = BuildScoreText(m.Score);
                    var team1Img = m.TeamInfo?.ElementAtOrDefault(0)?.Img;
                    var team2Img = m.TeamInfo?.ElementAtOrDefault(1)?.Img;

                    return new CricketMatch
                    {
                        MatchId = m.Id,
                        Name = m.Name,
                        Team1 = m.Teams?.ElementAtOrDefault(0) ?? "TBD",
                        Team2 = m.Teams?.ElementAtOrDefault(1) ?? "TBD",
                        Team1Img = team1Img,
                        Team2Img = team2Img,
                        Status = m.Status ?? "Unknown",
                        Score = scoreText,
                        MatchType = m.MatchType?.ToUpper() ?? "N/A",
                        StartTime = DateTime.TryParse(m.DateTimeGMT, out var dt) ? dt : null,
                        Venue = m.Venue,
                        MatchStarted = m.MatchStarted,
                        MatchEnded = m.MatchEnded
                    };
                }).ToList();

                return new MatchListResponse
                {
                    Matches = matches,
                    Success = true,
                    Message = $"Retrieved {matches.Count} matches"
                };
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