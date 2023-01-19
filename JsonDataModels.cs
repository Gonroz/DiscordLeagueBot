using System;

namespace DiscordLeagueBot;

public class SummonerV4
{
    public string id { get; set; }
    public string accountId { get; set; }
    public string puuid { get; set; }
    public string name { get; set; }
    public int profileIconId { get; set; }
    //public DateTime revisionDate { get; set; }
    public int summonerLevel { get; set; }
    
    public SummonerV4() {}
}

public class MatchV5
{
    public MatchV5Metadata metadata { get; set; }
    public MatchV5Info info { get; set; }
}

public class MatchV5Metadata
{
    public string matchId { get; set; }
    //public string[] participants { get; set; }
    //public MatchV5Info info { get; set; }
}

public class MatchV5Info
{
    public string? gameMode { get; set; }
    public MatchV5Participant[]? participants { get; set; }
}

public class MatchV5Participant
{
    public int assists { get; set; }
    public int deaths { get; set; }
    public bool gameEndedInSurrender { get; set; }
    public int kills { get; set; }
    public string puuid { get; set; }
    public string summonerName { get; set; }
    public bool win { get; set; }
}