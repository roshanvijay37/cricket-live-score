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
        public string? TossWinner { get; set; }
        public string? TossChoice { get; set; }
        public List<ScoreDetail>? ScoreDetails { get; set; }
        public List<TeamSquad>? Squads { get; set; }
    }

    public class ScoreDetail
    {
        public string? Inning { get; set; }
        public int Runs { get; set; }
        public int Wickets { get; set; }
        public double Overs { get; set; }
    }

    public class TeamSquad
    {
        public string? TeamName { get; set; }
        public string? Shortname { get; set; }
        public string? Img { get; set; }
        public List<Player>? Players { get; set; }
    }

    public class Player
    {
        public string? Name { get; set; }
        public string? Role { get; set; }
        public string? BattingStyle { get; set; }
        public string? BowlingStyle { get; set; }
        public string? PlayerImg { get; set; }
    }

    public class MatchListResponse
    {
        public List<CricketMatch> Matches { get; set; } = new List<CricketMatch>();
        public bool Success { get; set; }
        public string? Message { get; set; }
    }

    public class MatchDetailResponse
    {
        public CricketMatch? Match { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; }
    }

    // Models to deserialize CricAPI response
    public class CricApiResponse
    {
        public List<CricApiMatch>? Data { get; set; }
        public string? Status { get; set; }
    }

    public class CricApiSingleResponse
    {
        public CricApiMatch? Data { get; set; }
        public string? Status { get; set; }
    }

    public class CricApiSquadResponse
    {
        public List<CricApiSquad>? Data { get; set; }
        public string? Status { get; set; }
    }

    public class CricApiSquad
    {
        public string? TeamName { get; set; }
        public string? Shortname { get; set; }
        public string? Img { get; set; }
        public List<CricApiPlayer>? Players { get; set; }
    }

    public class CricApiPlayer
    {
        public string? Name { get; set; }
        public string? Role { get; set; }
        public string? BattingStyle { get; set; }
        public string? BowlingStyle { get; set; }
        public string? PlayerImg { get; set; }
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
        public string? TossWinner { get; set; }
        public string? TossChoice { get; set; }
        public bool HasSquad { get; set; }
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