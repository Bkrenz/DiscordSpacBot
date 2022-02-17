using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.WebSocket;
using Discord.Commands;

using DiscordSpacBot.Commands;

using AlphaVantage.Net.Core.Client;

namespace DiscordSpacBot
{
    class Program
    {
        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();


        private DiscordSocketClient _client;
        private CommandService _commands;
        private CommandHandler _commandHandler;
        private IServiceProvider _services;

        public async Task MainAsync()
        {
            // Set up the client
            _client = new DiscordSocketClient();
            _client.Log += Log;

            // Set up the commands
            _commands = new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Info,
                CaseSensitiveCommands = false

            });
            _commands.Log += Log;

            // Set up services
            _services = ConfigureServices();


            // Load Commands
            _commandHandler = new CommandHandler(_client, _services, _commands);
            await _commandHandler.InstallCommandsAsync();

            // TODO: Move this to somewhere safe
            var token = "insert-token";

            // Log the bot in and start it
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();


            // Block this task until the program is closed;
            await Task.Delay(-1);

        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private IServiceProvider ConfigureServices()
        {
            var map = new ServiceCollection();

            // Setup AlphaVantage Stock API
            string api_key = "9RKAEL1DHQXGMP5F";
            AlphaVantageClient _client = new AlphaVantageClient(api_key);
            map.AddSingleton(_client);

            return map.BuildServiceProvider();
        }

    }
}
