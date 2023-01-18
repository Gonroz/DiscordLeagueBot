using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Discord;
using Discord.WebSocket;

namespace DiscordLeagueBot;

public class DiscordBot
{
    private InsultGenerator _insultGenerator = new();
    private RiotApiCallHandler _riotApiCallHandler = new();
    private SQLiteDatabaseHandler _databaseHandler;

    public DiscordBot()
    {
        _insultGenerator.UpdateWordListFiles();

        _riotApiCallHandler.UpdateApiKey(@"ApiKeys\RiotApiKey.txt");

        var databaseLocation = "./Database.db";
        _databaseHandler = new SQLiteDatabaseHandler(databaseLocation);
    }

    /// <summary>
    /// Make the bot respond with 'Pong!'.
    /// </summary>
    /// <returns>'Pong!'.</returns>
    public async Task<string> PingPong()
    {
        return "Pong!";
    }
    
    /// <summary>
    /// Make the bot roast a specified user.
    /// </summary>
    /// <param name="user">The discord user to be roasted.</param>
    /// <returns>A randomized roast.</returns>
    public async Task<string> Roast(IUser user)
    {
        return await _insultGenerator.GenerateRandomInsult(user.Mention);
    }

    public async Task<string> LinkRiotToDiscord(SocketSlashCommand command)
    {
        var response = "";
        try
        {
            var summonerName = (string)command.Data.Options.First().Value;
            await _databaseHandler.RegisterDiscordAndRiotAccount(command.User, summonerName);
            response = $"Added {command.User.Mention} to the database!";
        }
        catch (Exception e)
        {
            response = $"Failed to link riot to discord with error: {e.Message}";
            if (e.InnerException != null)
            {
                response += $" Inner exception: {e.InnerException.Message}";
            }
        }

        return response;
    }

    public async Task<string> ShowKDA()
    {
        var response = "";

        try
        {
            var jsonText = await _riotApiCallHandler.GetMatchV5JsonWithMatchId("NA1_4487433350");
            MatchV5? match = JsonSerializer.Deserialize<MatchV5>(jsonText);
            response = match?.info.gameMode ?? "null";
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return response;
    }
}