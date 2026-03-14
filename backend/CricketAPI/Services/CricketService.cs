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

        public CricketService(HttpClient httpClient, ILogger<CricketService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<MatchListResponse> GetLiveMatchesAsync()
        {
            try
            {
                // Simulate dynamic scores for demo
                var random = new Random();
                var currentTime = DateTime.Now;
                
                var mockMatches = new List<CricketMatch>
                {
                    new CricketMatch
                    {
                        MatchId = "1",
                        Team1 = "India",
                        Team2 = "Australia",
                        Status = "Live",
                        Score = $"IND {245 + random.Next(0, 20)}/{4 + random.Next(0, 3)} ({45 + random.Next(0, 5)}.{random.Next(0, 6)}) vs AUS {198 + random.Next(0, 30)}/{8 + random.Next(0, 2)} ({40 + random.Next(0, 8)}.{random.Next(0, 6)})",
                        MatchType = "ODI",
                        StartTime = currentTime.AddHours(-2),
                        Venue = "Melbourne Cricket Ground"
                    },
                    new CricketMatch
                    {
                        MatchId = "2",
                        Team1 = "England",
                        Team2 = "Pakistan",
                        Status = "Upcoming",
                        Score = "Match starts in 2 hours",
                        MatchType = "T20",
                        StartTime = currentTime.AddHours(2),
                        Venue = "Lord's Cricket Ground"
                    }
                };

                return new MatchListResponse
                {
                    Matches = mockMatches,
                    Success = true,
                    Message = "Live data retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching cricket matches");
                return new MatchListResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }
    }
}