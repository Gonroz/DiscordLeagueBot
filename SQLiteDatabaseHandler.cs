using System;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;
using Discord;
using Discord.Net;

namespace DiscordLeagueBot;

public class SQLiteDatabaseHandler
{
    string databaseFileLocation;
    private SqliteConnectionStringBuilder _connectionString = new();
    private RiotApiCallHandler _riotApiCallHandler = new();
    
    public SQLiteDatabaseHandler()
    {
        //_riotApiCallHandler.UpdateApiKey("ApiKeys/RiotApiKey.txt");
    }

    public SQLiteDatabaseHandler(string fileLocation)
    {
        databaseFileLocation = fileLocation;
        _connectionString.DataSource = fileLocation;
    }

    public async Task Start()
    {
        await _riotApiCallHandler.UpdateApiKey("ApiKeys/RiotApiKey.txt");
    }

    public async Task<string> SQLiteTest()
    {
        var connectionStringBuilder = new SqliteConnectionStringBuilder();
        connectionStringBuilder.DataSource = "./myDB.db";

        var s = "No return string";

        await using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
        {
            connection.Open();
            
            // create table
            var tableCommand = connection.CreateCommand();
            tableCommand.CommandText = "CREATE TABLE favorite_beers(name varchar(50));";
            tableCommand.ExecuteNonQuery();
            
            // insert some records
            await using (var transaction = connection.BeginTransaction())
            {
                var insertCommand = connection.CreateCommand();

                insertCommand.CommandText = "INSERT INTO favorite_beers VALUES('LAGUNITAS IPA');";
                insertCommand.ExecuteNonQuery();
                
                insertCommand.CommandText = "INSERT INTO favorite_beers VALUES('CORONA');";
                insertCommand.ExecuteNonQuery();
                
                insertCommand.CommandText = "INSERT INTO favorite_beers VALUES('HAYBURNER');";
                insertCommand.ExecuteNonQuery();
                
                transaction.Commit();
            }
            
            // read records
            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText = "SELECT * FROM favorite_beers;";
            await using (var reader = await selectCommand.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    s = reader.GetString(0);
                    Console.WriteLine(s);
                }
            }
        }

        return s;
    }

    /// <summary>
    /// Registers a discord account into the SQLite database.
    /// </summary>
    /// <param name="user">The discord user IUser that will be added.</param>
    public async Task RegisterDiscordAccountToDatabase(IUser user)
    {
        var discordId = user.Id;
        var discordUsername = user.Username;
        var commandText = "";
        
        Console.WriteLine($"id: {discordId} username: {discordUsername}");
        await using (var connection = new SqliteConnection(_connectionString.ConnectionString))
        {
            connection.Open();
            
            Console.WriteLine("Connection open");

            await using (var transaction = connection.BeginTransaction())
            {
                Console.WriteLine("Transaction is open");
                var insertCommand = connection.CreateCommand();
                // $"INSERT INTO users (discord_id, discord_username) VALUES ({discordID}, {discordUsername});"
                commandText = $@"INSERT INTO users (discord_id,discord_username)
                                VALUES ({discordId},'{discordUsername}');";
                Console.WriteLine(commandText);
                insertCommand.CommandText = commandText;
                insertCommand.ExecuteNonQuery();
                
                Console.WriteLine($"part2 - id: {discordId} username: {discordUsername}");
                
                transaction.Commit();
                Console.WriteLine("Transaction committed");
            }
        }
    }

    /// <summary>
    /// Registers a discord account and riot account into the SQLite database. They are linked together.
    /// </summary>
    /// <param name="user">The discord user being added.</param>
    /// <param name="riotUsername">The riot username being added.</param>
    /// <exception cref="Exception">Throws whatever exception that may occur.</exception>
    public async Task RegisterDiscordAndRiotAccount(IUser user, string riotUsername)
    {
        var discordId = user.Id;
        var discordUsername = user.Username;
        var riotPuuid = await _riotApiCallHandler.GetPuuidFromUsername(riotUsername);
        var commandText = "";
        
        await using (var connection = new SqliteConnection(_connectionString.ConnectionString))
        {
            connection.Open();
            
            Console.WriteLine("Connection open registerdiscordandriot");

            try
            {
                await using (var transaction = connection.BeginTransaction())
                {
                    Console.WriteLine("Using var transaction");
                    //try
                    //{
                        Console.WriteLine("Transaction is open");
                        var insertCommand = connection.CreateCommand();
                        // $"INSERT INTO users (discord_id, discord_username) VALUES ({discordID}, {discordUsername});"
                        commandText = $@"
                            INSERT INTO users (discord_id,discord_username, riot_puuid, riot_username)
                            VALUES ({discordId},'{discordUsername}', '{riotPuuid}', '{riotUsername}');";
                        Console.WriteLine(commandText);
                        insertCommand.CommandText = commandText;
                        insertCommand.ExecuteNonQuery();

                        Console.WriteLine($"part2 - id: {discordId} username: {discordUsername}");

                        transaction.Commit();
                        Console.WriteLine("Transaction committed");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw new Exception(e.Message, e);
            }
        }
    }

    /// <summary>
    /// Get the unique riot puuid from the database with a discord id.
    /// </summary>
    /// <param name="discordId">The discord account id.</param>
    /// <returns>A string of the riot puuid.</returns>
    /// <exception cref="Exception">Throws the exception that caused the puuid to fail to get.</exception>
    public async Task<string> GetPuuid(ulong discordId)
    {
        var puuid = "";
        try
        {
            await using (var connection = new SqliteConnection(_connectionString.ConnectionString))
            {
                connection.Open();
                var selectPuuidCommand = connection.CreateCommand();
                selectPuuidCommand.CommandText = $@"SELECT riot_puuid
                                                    FROM users
                                                    WHERE discord_id = '{discordId}';";
                await using (var reader = await selectPuuidCommand.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        puuid = reader.GetString(0);
                        Console.WriteLine(puuid);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception("Failure to get puuid.", e);
        }
        return puuid;
    }

    /// <summary>
    /// Writes a string of the match id history into the database for the account with the discord id.
    /// </summary>
    /// <param name="discordId">The discord id to add the match history to.</param>
    /// <exception cref="Exception">Throws whatever exception may occur.</exception>
    public async Task WriteMatchIdHistory(ulong discordId)
    {
        //await _riotApiCallHandler.UpdateApiKey("ApiKeys/RiotApiKey.txt");
        var commandText = "";
        var puuid = await GetPuuid(discordId);
        var matchIdHistory = await _riotApiCallHandler.GetMatchIdHistoryWithPuuid(puuid);
        Console.WriteLine($"ids: {matchIdHistory}");
        try
        {
            await using var connection = new SqliteConnection(_connectionString.ConnectionString);
            connection.Open();
            await using var transaction = connection.BeginTransaction();
            var insertCommand = connection.CreateCommand();
            commandText = $@"UPDATE users
                            SET match_id_history = '{matchIdHistory}'
                            WHERE discord_id = '{discordId}';";
            insertCommand.CommandText = commandText;
            insertCommand.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception("Failed to write match ID history to database", e);
        }
    }

    public async Task WriteMatchIdHistory(ulong discordId, string gameType)
    {
        //await _riotApiCallHandler.UpdateApiKey("ApiKeys/RiotApiKey.txt");
        var commandText = "";
        var puuid = await GetPuuid(discordId);
        var matchIdHistory = await _riotApiCallHandler.GetMatchIdHistoryWithPuuid(puuid);
        
        // this part is new
        string matchedGameIds = "matchedGameIds: ";
        int matches = 0;
        string[]? matchIds = JsonSerializer.Deserialize<string[]>(matchIdHistory);
        foreach (var id in matchIds)
        {
            var json = await _riotApiCallHandler.GetMatchV5JsonWithMatchId(id);
            MatchV5? match = JsonSerializer.Deserialize<MatchV5>(json);
            if (match.info.gameType != gameType)
            {
                matchedGameIds += match.metadata.matchId + ",";
                matches++;
            }
        }
        Console.WriteLine($"match ids: {matchedGameIds} count:{matches}");

        try
        {
            await using var connection = new SqliteConnection(_connectionString.ConnectionString);
            connection.Open();
            await using var transaction = connection.BeginTransaction();
            var insertCommand = connection.CreateCommand();
            commandText = $@"UPDATE users
                            SET match_id_history = '{matchIdHistory}'
                            WHERE discord_id = '{discordId}';";
            insertCommand.CommandText = commandText;
            insertCommand.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception("Failed to write match ID history to database", e);
        }
    }
}