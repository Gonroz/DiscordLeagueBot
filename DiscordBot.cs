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

    /// <summary>
    /// Link their riot account to their discord account.
    /// </summary>
    /// <param name="command">The slash command that was used.</param>
    /// <returns>A response saying either it was successful or that an error has occured.</returns>
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

    public async Task<string> ShowKDA(ulong discordId)
    {
        var response = "";

        try
        {
            var jsonText = await _riotApiCallHandler.GetMatchV5JsonWithMatchId("NA1_4487433350");
            MatchV5? match = JsonSerializer.Deserialize<MatchV5>(jsonText);
            response = match?.info.gameMode ?? "null";

            await _databaseHandler.WriteMatchIdHistoryToDatabaseWithDiscordId(discordId);
            var puuid = await _databaseHandler.GetPuuid(discordId);
            foreach (var participant in match.info.participants)
            {
                Console.WriteLine(participant.puuid);
                if (participant.puuid == puuid)
                {
                    Console.WriteLine("match");
                    double kills = participant.kills;
                    double deaths = participant.deaths;
                    return (kills / deaths).ToString();
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return response;
    }

    /// <summary>
    /// Get kda of a specific player in a match.
    /// </summary>
    /// <param name="matchId">The id of the match.</param>
    /// <param name="discordId">The discord id of the player.</param>
    /// <returns>Their kda. Returns -1 if an error occured.</returns>
    public async Task<double> GetMatchKda(ulong discordId, string matchId)
    {
        try
        {
            var jsonText = await _riotApiCallHandler.GetMatchV5JsonWithMatchId(matchId);
            var puuid = await _databaseHandler.GetPuuid(discordId);
            
            MatchV5 match = JsonSerializer.Deserialize<MatchV5>(jsonText);

            foreach (var participant in match.info.participants)
            {
                if (participant.puuid == puuid)
                {
                    double kills = participant.kills;
                    double deaths = participant.deaths;
                    double assists = participant.assists;
                    double kda = deaths != 0 ? (kills + assists) / deaths : kills + assists;
                    return kda;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return -1;
    }
}