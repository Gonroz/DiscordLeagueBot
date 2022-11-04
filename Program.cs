﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;

namespace DiscordLeagueBot
{
    class Program
    {
        public static void Main(string[] args)
        => new Program().MainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient? _client;
        private ISocketMessageChannel? _channel;

        private RiotApiCallHandler? _riotApiCallHandler = new();
        private SQLiteDatabaseHandler? _databaseHandler = new();

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();
            _client.Log += Log;
            _client.Ready += ClientReady;
            _client.SlashCommandExecuted += SlashCommandHandler;

            var token = "MTAzMDAwNzIyNzA4MTMwNjE3Mg.G8JNY7.lXbIOxD6PwOhNs7ERlO6DDCSv_xApoc2pIOV5g";
            //var riotApiKey = "RGAPI-9831f7f3-e445-4fc0-9a0f-61fc864c3993";
            //_riotApiCallHandler = new RiotApiCallHandler(riotApiKey);

            var databaseLocation = "./Database.db";
            _databaseHandler = new SQLiteDatabaseHandler(databaseLocation);

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            while (_client.Status == UserStatus.Online)
            {
                await Task.Delay(5000);
                //Console.WriteLine("awake");
                if (_channel != null)
                {
                    //Console.WriteLine(channel.Name);
                    await _channel.SendMessageAsync("Aw fugg baby I'm wet");
                }
            }
            
            await Task.Delay(-1);
            //await client.StopAsync();
        }

        private Task Log(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }

        private async Task ClientReady()
        {
            var applicationCommandProperties = new List<ApplicationCommandProperties>();
            var guildCommands = new List<SlashCommandBuilder>();
            
            ulong developmentGuildId = 468870202491404290; // BotTesting discord
            var developmentGuild = _client.GetGuild(developmentGuildId);
            
            // Guild commands
            var guildSummonerNameCommand = new SlashCommandBuilder()
                .WithName("get-summoner-name")
                .WithDescription("Get summoner ID from name")
                .AddOption("summoner-name", ApplicationCommandOptionType.String, "Name of summoner.", isRequired: true);
            guildCommands.Add(guildSummonerNameCommand);

            var guildTestSQLiteCommand = new SlashCommandBuilder()
                .WithName("sqlite-test")
                .WithDescription("Test SQLite");
            guildCommands.Add(guildTestSQLiteCommand);

            var guildRegisterDiscordCommand = new SlashCommandBuilder()
                .WithName("register-discord")
                .WithDescription("Add your discord to the database");
            guildCommands.Add(guildRegisterDiscordCommand);

            var guildRegisterRiotToDiscordCommand = new SlashCommandBuilder()
                .WithName("link-riot-to-discord")
                .WithDescription("Link your riot account to your discord account.")
                .AddOption("summoner-name", ApplicationCommandOptionType.String, "Name of summoner.", isRequired: true);
            guildCommands.Add(guildRegisterRiotToDiscordCommand);

            var guildGetMatchIdHistory = new SlashCommandBuilder()
                .WithName("get-match-id-history")
                .WithDescription("Get match id history of your account.");
            guildCommands.Add(guildGetMatchIdHistory);

            // Global commands
            var globalCommand = new SlashCommandBuilder()
                .WithName("first-global-command")
                .WithDescription("This is my first global slash command poop");
            applicationCommandProperties.Add(globalCommand.Build());

            var pingCommand = new SlashCommandBuilder()
                .WithName("ping")
                .WithDescription("Bot replies with pong!");
            //pingCommand.AddOption("poop", ApplicationCommandOptionType.Number, "poop backwards", isRequired: true);
            applicationCommandProperties.Add(pingCommand.Build());

            var setChannelCommand = new SlashCommandBuilder()
                .WithName("set-active-channel")
                .WithDescription("Allow the bot to run in the channel you send this message.");
            applicationCommandProperties.Add(setChannelCommand.Build());

            /*var summonerNameCommand = new SlashCommandBuilder()
                .WithName("get-summoner-name")
                .WithDescription("Get summoner string by name.")
                .AddOption("summonername", ApplicationCommandOptionType.String, "Enter your summoner name.", isRequired: true);
            applicationCommandProperties.Add(summonerNameCommand.Build());*/

            /*var logoffCommand = new SlashCommandBuilder()
                .WithName("logoff-bot")
                .WithDescription("Command to log off the bot.");
            applicationCommandProperties.Add(logoffCommand.Build());*/

            try
            {
                // Now that we have our builder, we can call the CreateApplicationCommandAsync method to make our slash command.
                //await guild.CreateApplicationCommandAsync(guildCommand.Build());

                // With global commands we don't need the guild.
                //await client.CreateGlobalApplicationCommandAsync(globalCommand.Build());
                //await client.CreateGlobalApplicationCommandAsync(pingCommand.Build());
                // Using the ready event is a simple implementation for the sake of the example. Suitable for testing and development.
                // For a production bot, it is recommended to only run the CreateGlobalApplicationCommandAsync() once for each command.
                await _client.BulkOverwriteGlobalApplicationCommandsAsync(applicationCommandProperties.ToArray());
                
                // Guild commands
                await developmentGuild.DeleteApplicationCommandsAsync();
                foreach (var command in guildCommands)
                {
                    await developmentGuild.CreateApplicationCommandAsync(command.Build());
                }
                //await developmentGuild.CreateApplicationCommandAsync(guildSummonerNameCommand.Build());
            }
            catch(HttpException exception)
            {
                // If our command was invalid, we should catch an ApplicationCommandException. This exception contains the path of the error as well as the error message. You can serialize the Error field in the exception to get a visual of where your error is.
                //var json = JsonConvert.SerializeObject(exception.Error, Formatting.Indented);
                Console.WriteLine(exception.ToString());

                // You can send this error somewhere or just print it to the console, for this example we're just going to print it.
                //Console.WriteLine(json);
                Console.WriteLine("Error");
            }
        }

        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            var response = "Generic message response";
            switch (command.Data.Name)
            {
                case "ping":
                    await command.RespondAsync("Pong!"); 
                    break;
                
                case "set-active-channel":
                    _channel = command.Channel;
                    await command.RespondAsync($"Set allowed channel to {command.Channel.Name} with id {_channel.Id}");
                    break;
                
                case "get-summoner-name":
                    try
                    {
                        var summonerName = (string)command.Data.Options.First().Value;
                        response = await _riotApiCallHandler.GetSummonerByNameAsync(summonerName);
                    }
                    catch (Exception e)
                    {
                        response = $"Failed to get summoner by name with error: {e.Message}";
                    }
                    await command.RespondAsync(response);
                    break;
                
                case "link-riot-to-discord":
                    try
                    {
                        var summonerName = (string)command.Data.Options.First().Value;
                        await _databaseHandler.RegisterDiscordAndRiotAccount(command.User, summonerName);
                        response = $"Added {command.User.Mention} to the database!";
                        //await _riotApiCallHandler.GetPuuidFromUsername(summonerName);
                    }
                    catch (Exception e)
                    {
                        response = $"Failed to link riot to discord with error: {e.Message}";
                        if (e.InnerException != null)
                        {
                            response += $" Inner exception: {e.InnerException.Message}";
                        }
                    }
                    await command.RespondAsync(response);
                    break;
                
                case "get-match-id-history":
                    try
                    {
                        var puuid = await _databaseHandler.GetPuuidFromDatabaseWithDiscordUser(command.User);
                        var idHistory = await _riotApiCallHandler.GetMatchIdHistory(puuid);
                        foreach (var match in idHistory)
                        {
                            response += match;
                        }
                    }
                    catch (Exception e)
                    {
                        response = e.InnerException == null
                            ? $"Exception: {e.Message}"
                            : $"Exception: {e.Message} Inner: {e.InnerException.Message}";
                    }
                    await command.RespondAsync(response);
                    break;
            }

            if (command.Data.Name == "sqlite-test")
            {
                // create database
                try
                {
                    response = await _databaseHandler.SQLiteTest();
                }
                catch (Exception e)
                {
                    response = $"Failed to create table with error: {e.Message}";
                }

                await command.RespondAsync(response);
            }

            if (command.Data.Name == "register-discord")
            {
                try
                {
                    await _databaseHandler.RegisterDiscordAccountToDatabase(command.User);
                    response = $"Added {command.User.Mention} to the database!";
                }
                catch (Exception e)
                {
                    response = $"Failed to register discord with error: {e.Message}";
                }

                await command.RespondAsync(response);
            }

            if (command.Data.Name == "logoff-bot")
            {
                await _client.LogoutAsync();
            }

            // If no other specialties for a command happens
            //else
                //await command.RespondAsync($"You executed {command.Data.Name} GENERIC RESPONSE");
        }
    }
}