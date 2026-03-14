namespace CricketAPI.Models
{
    public class CricketMatch
    {
        public string? MatchId { get; set; }
        public string? Name { get; set; }
        public string? Team1 { get; set; }
        public string? Team2 { get; set; }
        public string? Team1Img { get; set; }
        public string? Team2Img { get; set; }
        public string? Status { get; set; }
        public string? Score { get; set; }
        public string? MatchType { get; set; }
        public DateTime? StartTime { get; set; }
        public string? Venue { get; set; }
        public bool MatchStarted { get; set; }
        public bool MatchEnded { get; set; }
    }

    public class MatchListResponse
    {
        public List<CricketMatch> Matches { get; set; } = new List<CricketMatch>();
        public bool Success { get; set; }
        public string? Message { get; set; }
    }

    // Models to deserialize CricAPI response
    public class CricApiResponse
    {
        public List<CricApiMatch>? Data { get; set; }
        public string? Status { get; set; }
    }

    public class CricApiMatch
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? MatchType { get; set; }
        public string? Status { get; set; }
        public string? Venue { get; set; }
        public string? DateTimeGMT { get; set; }
        public List<string>? Teams { get; set; }
        public List<CricApiTeamInfo>? TeamInfo { get; set; }
        public List<CricApiScore>? Score { get; set; }
        public bool MatchStarted { get; set; }
        public bool MatchEnded { get; set; }
    }

    public class CricApiTeamInfo
    {
        public string? Name { get; set; }
        public string? Shortname { get; set; }
        public string? Img { get; set; }
    }

    public class CricApiScore
    {
        public int R { get; set; }
        public int W { get; set; }
        public double O { get; set; }
        public string? Inning { get; set; }
    }
}