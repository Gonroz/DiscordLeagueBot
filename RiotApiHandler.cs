using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace DiscordLeagueBot;

public class RiotApiCallHandler
{
    private HttpClient _httpClient= new();
    private string _riotApiKey = "RGAPI-d847892c-334e-4011-9cb2-83d6a0e2cf89";
    
    public RiotApiCallHandler()
    {
        
    }

    public RiotApiCallHandler(string apiKey)
    {
        _riotApiKey = apiKey;
    }

    public async Task<string> GetJsonStringFromUrlAsync(string url)
    {
        try
        {
            var response = await _httpClient.GetStringAsync(url);
            return response;
        }
        catch (HttpRequestException e)
        {
            return e.Message;
        }
    }

    // Get summoner JSON from summonerV4 on riot developer portal
    public async Task<string> GetSummonerByNameAsync(string summonerName)
    {
        var response = "Response.";
        try
        {
            response = await GetJsonStringFromUrlAsync(
                $"https://na1.api.riotgames.com/lol/summoner/v4/summoners/by-name/{summonerName}?api_key={_riotApiKey}");
            return response;
        }
        catch (Exception e)
        {
            response = $"Failed to get summoner by name with error: {e.Message}";
            return response;
        }

        return response;
    }

    // Get puuid from a riot username
    public async Task<string> GetPuuidFromUsername(string summonerName)
    {
        Console.WriteLine(summonerName);
        try
        {
            var jsonText = await GetSummonerByNameAsync(summonerName);
            var summoner = JsonSerializer.Deserialize<SummonerV4>(jsonText);
            Console.WriteLine(summoner.puuid);
            return summoner.puuid;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return "Failure to get puuid";
        }
    }

    public async Task<string[]> GetMatchIdHistory(string puuid)
    {
        try
        {
            var url = $"https://na1.api.riotgames.com/lol/match/v5/matches/by-puuid/{puuid}/ids";
            var jsonText = await GetJsonStringFromUrlAsync(url);
            var matchIdArray = JsonSerializer.Deserialize<string[]>(jsonText);
            return matchIdArray;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception("Failed to get match id history.", e);
        }
    }
}