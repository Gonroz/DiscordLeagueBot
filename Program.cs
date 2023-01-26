using System;
using System.Collections.Generic;
using System.IO;
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

        private DiscordSocketClient _client = new();
        private ISocketMessageChannel? _channel;

        private RiotApiCallHandler _riotApiCallHandler = new();
        private SQLiteDatabaseHandler _databaseHandler = new();

        //private InsultGenerator _insultGenerator = new();

        private DiscordBot _discordBot = new();
        
        private string _token = "token";
        private string _discordTokenPath = "ApiKeys/DiscordApiKey.txt";

        private string _riotApiKeyPath = "ApiKeys/RiotApiKey.txt";

        public async Task MainAsync()
        {
            //_client = new DiscordSocketClient();
            _client.Log += Log;
            _client.Ready += ClientReady;
            _client.SlashCommandExecuted += SlashCommandHandler;
            
            // InsultGenerator type stuff
            //await _insultGenerator.UpdateWordListFiles();
            
            await UpdateApiKeys();

            var databaseLocation = "./Database.db";
            _databaseHandler = new SQLiteDatabaseHandler(databaseLocation);
            
            await _discordBot.Start();
            await _databaseHandler.Start();

            await _client.LoginAsync(TokenType.Bot, _token);
            await _client.StartAsync();

            while (_client.Status == UserStatus.Online)
            {
                await Task.Delay(5000);
                //Console.WriteLine("awake");
                if (_channel != null)
                {
                    //Console.WriteLine(channel.Name);
                    await _channel.SendMessageAsync("Running");
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

        private async Task UpdateApiKeys()
        {
            if (File.Exists(_discordTokenPath))
            {
                var text = await File.ReadAllLinesAsync(_discordTokenPath);
                _token = text[1];
            }
            else
            {
                Console.WriteLine($"File not found at: {_discordTokenPath}");
            }

            //await _riotApiCallHandler.UpdateApiKey(_riotApiKeyPath);
        }

        private async Task ClientReady()
        {
            var applicationCommandProperties = new List<ApplicationCommandProperties>();
            var guildCommands = new List<SlashCommandBuilder>();
            
            ulong developmentGuildId = 468870202491404290; // BotTesting discord
            //var developmentGuild = _client.GetGuild(developmentGuildId);
            
            // Guild commands
            var guildSummonerNameCommand = new SlashCommandBuilder()
                .WithName("get-summoner-name")
                .WithDescription("Get summoner ID from name")
                .AddOption("summoner-name", ApplicationCommandOptionType.String, "Name of summoner.", isRequired: true);
            guildCommands.Add(guildSummonerNameCommand);

            /*var guildTestSQLiteCommand = new SlashCommandBuilder()
                .WithName("sqlite-test")
                .WithDescription("Test SQLite");
            guildCommands.Add(guildTestSQLiteCommand);*/

            var guildRegisterDiscordCommand = new SlashCommandBuilder()
                .WithName("register-discord")
                .WithDescription("Add your discord to the database");
            guildCommands.Add(guildRegisterDiscordCommand);

            var guildRegisterRiotToDiscordCommand = new SlashCommandBuilder()
                .WithName("link-riot-to-discord")
                .WithDescription("Link your riot account to your discord account.")
                .AddOption("summoner-name", ApplicationCommandOptionType.String, "Name of summoner.", isRequired: true);
            guildCommands.Add(guildRegisterRiotToDiscordCommand);

            var guildGetMatchIdHistoryCommand = new SlashCommandBuilder()
                .WithName("get-match-id-history")
                .WithDescription("Get match id history of your account.");
            guildCommands.Add(guildGetMatchIdHistoryCommand);

             /*var guildMatchV5TestCommand = new SlashCommandBuilder()
                .WithName("match-v5-test")
                .WithDescription("Match V5 Test.");
            guildCommands.Add(guildMatchV5TestCommand);*/

             var guildDevelopmentSubCommandGroup = new SlashCommandBuilder()
                 .WithName("development")
                 .WithDescription("Development sub command group")
                 .AddOption(new SlashCommandOptionBuilder()
                     .WithName("match-v5-test")
                     .WithDescription("match v5 test")
                     .WithType(ApplicationCommandOptionType.SubCommand)
                 )
                 .AddOption(new SlashCommandOptionBuilder()
                     .WithName("insult-test")
                     .WithDescription("Insult test")
                     .WithType(ApplicationCommandOptionType.SubCommand)
                 )
                 .AddOption(new SlashCommandOptionBuilder()
                     .WithName("roast")
                     .WithDescription("Make the bot roast someone.")
                     .AddOption("user", ApplicationCommandOptionType.User, "User you want to roast", isRequired: true)
                     .WithType(ApplicationCommandOptionType.SubCommand)
                 )
                 .AddOption(new SlashCommandOptionBuilder()
                     .WithName("kda-test")
                     .WithDescription("Testing kda")
                     .WithType(ApplicationCommandOptionType.SubCommand)
                 )
                 .AddOption(new SlashCommandOptionBuilder()
                     .WithName("update-match-history")
                     .WithDescription("Update match history.")
                     .WithType(ApplicationCommandOptionType.SubCommand)
                 )
                 .AddOption(new SlashCommandOptionBuilder()
                     .WithName("win-loss")
                     .WithDescription("Win Loss streak test")
                     .WithType(ApplicationCommandOptionType.SubCommand)
                 );
             guildCommands.Add(guildDevelopmentSubCommandGroup);

            /*var guildTestCommand = new SlashCommandBuilder()
                .WithName("test")
                .WithDescription("Test.");
            guildCommands.Add(guildTestCommand);*/

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
                await UpdateGuildCommands(developmentGuildId, guildCommands);
                /*await developmentGuild.DeleteApplicationCommandsAsync();
                foreach (var command in guildCommands)
                {
                    await developmentGuild.CreateApplicationCommandAsync(command.Build());
                }*/
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

        private async Task UpdateGuildCommands(ulong guildId, List<SlashCommandBuilder> guildCommands)
        {
            var developmentGuild = _client.GetGuild(guildId);
            await developmentGuild.DeleteApplicationCommandsAsync();
            foreach (var command in guildCommands)
            {
                await developmentGuild.CreateApplicationCommandAsync(command.Build());
            }
        }

        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            var response = "Generic message response";

            switch (command.Data.Name)
            {
                case "development":
                    await HandleDevelopmentCommand(command);
                    break;
            }
            
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
                    response = await _discordBot.LinkRiotToDiscord(command);
                    await command.RespondAsync(response);
                    break;
                
                case "get-match-id-history":
                    try
                    {
                        var puuid = await _databaseHandler.GetPuuid(command.User.Id);
                        var idHistory = await _riotApiCallHandler.GetMatchIdHistoryWithPuuid(puuid);
                        await _databaseHandler.WriteMatchIdHistory(command.User.Id);
                        response = "";
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
                
                case "match-v5-test":
                    try
                    {
                        response = await _riotApiCallHandler.GetMatchV5JsonWithMatchId("NA1_4487433350");
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

        private async Task HandleDevelopmentCommand(SocketSlashCommand command)
        {
            var response = "HandleDevelopmentCommand response";

            var specificCommandName = command.Data.Options.First().Name;

            switch (specificCommandName)
            {
                case "match-v5-test":
                    response = "It works";
                    break;
                
                case "insult-test":
                    //response = await _insultGenerator.GenerateRandomInsult();
                    //response = await _insultGenerator.GenerateRandomInsult(command.User.Mention);
                    response = "no longer a thing";
                    break;
                
                case "roast":
                    response = await _discordBot.Roast((IUser)command.Data.Options.First().Options.First().Value);
                    break;
                
                case "kda-test":
                    response = $"{await _discordBot.GetMatchKda(command.User.Id, "NA1_4487433350")}";
                    break;
                
                case "update-match-history":
                    //response = await _discordBot.UpdateMatchHistory(command.User.Id);
                    response = await _discordBot.UpdateMatchHistory(command.User.Id);
                    break;
                
                case "win-loss":
                    response = $"streak :{await _discordBot.WinLossStreak(command.User.Id)}";
                    break;
            }

            await command.RespondAsync(response);
        }
    }
}