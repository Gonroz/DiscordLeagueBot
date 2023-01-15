using System;
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
        
    }

    public SQLiteDatabaseHandler(string fileLocation)
    {
        databaseFileLocation = fileLocation;
        _connectionString.DataSource = fileLocation;
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
                commandText = $"INSERT INTO users (discord_id,discord_username) VALUES ({discordId},'{discordUsername}');";
                Console.WriteLine(commandText);
                insertCommand.CommandText = commandText;
                insertCommand.ExecuteNonQuery();
                
                Console.WriteLine($"part2 - id: {discordId} username: {discordUsername}");
                
                transaction.Commit();
                Console.WriteLine("Transaction committed");
            }
        }
    }

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

    public async Task<string> GetPuuidFromDatabaseWithDiscordId(ulong discordId)
    {
        var puuid = "";
        try
        {
            await using (var connection = new SqliteConnection(_connectionString.ConnectionString))
            {
                connection.Open();
                var selectPuuidCommand = connection.CreateCommand();
                selectPuuidCommand.CommandText = $@"SELECT riot_puuid FROM users WHERE discord_id = '{discordId}';";
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

    public async Task WriteMatchIdHistoryToDatabaseWithDiscordId(ulong discordId)
    {
        var commandText = "";
        var puuid = await GetPuuidFromDatabaseWithDiscordId(discordId);
        var matchIdHistory = await _riotApiCallHandler.GetMatchIdHistoryWithPuuid(puuid);
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