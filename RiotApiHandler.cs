using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace DiscordLeagueBot;

public class RiotApiCallHandler
{
    private HttpClient _httpClient= new();
    private string _riotApiKey = "riotApiKey";

    private string defaultFilePath = "ApiKeys/RiotApiKey.txt";
    
    public RiotApiCallHandler()
    {
        if (File.Exists(defaultFilePath))
        {
            var text = File.ReadAllLines(defaultFilePath);
            _riotApiKey = text[1];
            Console.WriteLine($"declaration: updateapikey key: {_riotApiKey}");
        }
        else
        {
            Console.WriteLine($"Failed to find file at: '{defaultFilePath}'");
        }
    }

    public RiotApiCallHandler(string apiKey)
    {
        _riotApiKey = apiKey;
    }

    /// <summary>
    /// Update the api key from the file that it is stored in. Make sure the key is on the second line.
    /// </summary>
    /// <param name="filePath">The path to the file containing the key.</param>
    public async Task UpdateApiKey(string filePath)
    {
        if (File.Exists(filePath))
        {
            var text = await File.ReadAllLinesAsync(filePath);
            _riotApiKey = text[1];
            Console.WriteLine($"updateapikey key: {_riotApiKey}");
        }
        else
        {
            Console.WriteLine($"Failed to find file at: '{filePath}'");
        }
    }

    /// <summary>
    /// Returns JSON from the url address.
    /// </summary>
    /// <param name="url">The url to get the JSON from.</param>
    /// <returns>The JSON is the form of a string.</returns>
    public async Task<string> GetJsonStringFromUrlAsync(string url)
    {
        Console.WriteLine($"getjson: {_riotApiKey}");
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
    
    /// <summary>
    /// Get summoner JSON from summonerV4 on riot developer portal.
    /// </summary>
    /// <param name="summonerName">The summoner name you want to get the JSON for.</param>
    /// <returns>The JSON in the form of a string.</returns>
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

    /// <summary>
    /// Get puuid from a summoner name.
    /// </summary>
    /// <param name="summonerName">The summoner name you want to get the puuid for.</param>
    /// <returns>The puuid in the form of a string.</returns>
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

    /// <summary>
    /// Get the match history JSON from a puuid.
    /// </summary>
    /// <param name="puuid">The puuid you want to get the match history for.</param>
    /// <returns>The JSON in the form of a string.</returns>
    /// <exception cref="Exception">Throws whatever exception may occur.</exception>
    public async Task<string> GetMatchIdHistoryWithPuuid(string puuid)
    {
        try
        {
            var url = $"https://americas.api.riotgames.com/lol/match/v5/matches/by-puuid/{puuid}/ids?start=0&count=20&api_key={_riotApiKey}";
            var jsonText = await GetJsonStringFromUrlAsync(url);
            //Console.WriteLine(url);
            //Console.WriteLine(_riotApiKey);
            return jsonText;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception("Failed to get match id history.", e);
        }
    }
    
    public async Task<string[]> GetMatchIdHistory(string puuid)
    {
        try
        {
            var url = $"https://americas.api.riotgames.com/lol/match/v5/matches/by-puuid/{puuid}/ids?start=0&count=20&api_key={_riotApiKey}";
            var jsonText = await GetJsonStringFromUrlAsync(url);
            //Console.WriteLine(jsonText);
            var matchIdArray = JsonSerializer.Deserialize<string[]>(jsonText);
            return matchIdArray;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception("Failed to get match id history.", e);
        }
    }

    /// <summary>
    /// Get matchV5 JSON from a certain match id.
    /// </summary>
    /// <param name="matchId">The particular match you want to get the JSON for.</param>
    /// <returns>The match JSON in the form of a string.</returns>
    /// <exception cref="Exception">Throws whatever exception may occur.</exception>
    public async Task<string> GetMatchV5JsonWithMatchId(string matchId)
    {
        try
        {
            var url = $"https://americas.api.riotgames.com/lol/match/v5/matches/{matchId}?api_key={_riotApiKey}";
            var jsonText = await GetJsonStringFromUrlAsync(url);
            //Console.WriteLine(jsonText);
            return jsonText;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception("Failed to get match v5.", e);
        }
    }
}