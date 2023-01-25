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
    private SQLiteDatabaseHandler _databaseHandler = new();

    public DiscordBot()
    {
    }

    public async Task Start()
    {
        await _insultGenerator.UpdateWordListFiles();
        
        //await _riotApiCallHandler.UpdateApiKey("ApiKeys/RiotApiKey.txt");

        await _databaseHandler.Start();
        
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
            
            MatchV5? match = JsonSerializer.Deserialize<MatchV5>(jsonText);

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

    public async Task<string> UpdateMatchHistory(ulong discordId)
    {
        await _databaseHandler.WriteMatchIdHistory(discordId, "MATCHED_GAME");
        return "Check database";
    }

    /// <summary>
    /// Get the win or loss streak of a discord user.
    /// </summary>
    /// <param name="discordId">The discord id of the user to check.</param>
    /// <returns>The streak as an integer. Positive is wins, negative is losses.</returns>
    public async Task<string> WinLossStreak(ulong discordId)
    {
        int streak = 0;
        bool winning = false;
        bool shouldBreak = false;
        
        try
        {
            var puuid = await _databaseHandler.GetPuuid(discordId);
            var matchIdsJson = await _riotApiCallHandler.GetMatchIdHistoryWithPuuid(puuid);
            Console.WriteLine(matchIdsJson);
            var matches = JsonSerializer.Deserialize<string[]>(matchIdsJson);

            var matchJson = await _riotApiCallHandler.GetMatchV5JsonWithMatchId(matches[0]);
            MatchV5? match = JsonSerializer.Deserialize<MatchV5>(matchJson);
            foreach (var participant in match.info.participants)
            {
                if (participant.puuid == puuid)
                {
                    winning = participant.win;
                }
            }
            
            foreach (var m in matches)
            {
                matchJson = await _riotApiCallHandler.GetMatchV5JsonWithMatchId(m);
                match = JsonSerializer.Deserialize<MatchV5>(matchJson);
                //Console.WriteLine(match.info.gameCreation);
                foreach (var participant in match.info.participants)
                {
                    if (participant.puuid == puuid)
                    {
                        if (participant.win && winning)
                        {
                            streak++;
                            Console.WriteLine("win");
                        }
                        else if (!participant.win && !winning)
                        {
                            streak--;
                            Console.WriteLine("loss");
                        }
                        else
                        {
                            //winning = false;
                            shouldBreak = true;
                        }
                        break;
                    }
                }

                if (shouldBreak)
                    break;
            }
            Console.WriteLine($"streak: {streak}");
            return $"win/loss streak is {streak}";
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}