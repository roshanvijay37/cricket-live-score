namespace CricketAPI.Models
{
    public class CricketMatch
    {
        public string? MatchId { get; set; }
        public string? Team1 { get; set; }
        public string? Team2 { get; set; }
        public string? Status { get; set; }
        public string? Score { get; set; }
        public string? MatchType { get; set; }
        public DateTime? StartTime { get; set; }
        public string? Venue { get; set; }
    }

    public class MatchListResponse
    {
        public List<CricketMatch> Matches { get; set; } = new List<CricketMatch>();
        public bool Success { get; set; }
        public string? Message { get; set; }
    }
}