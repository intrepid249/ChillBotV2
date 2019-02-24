using ChillBotV2.Context;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ChillBotV2
{
    public sealed class Core
    {
        private readonly IServiceProvider _services;
        private readonly CancellationTokenSource _cts;
        private readonly SemaphoreSlim _colorLock = new SemaphoreSlim(1, 1);

        public static IConfiguration _config;
        private readonly CommandService _commands = new CommandService(new CommandServiceConfig
        {
            LogLevel = LogSeverity.Debug,
            DefaultRunMode = RunMode.Async
        });
        private readonly DiscordSocketClient _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            MessageCacheSize = 100,
            LogLevel = LogSeverity.Debug
        });

        // Constructor
        public Core(CancellationTokenSource cts)
        {
            _cts = cts;

            _config = BuildConfig();

            _services = ConfigureServices();
        }

        public async Task InitialiseAsync()
        {
            _client.MessageReceived += HandleMessageReceived;
            _client.Ready += async () =>
            {
                await _client.SetGameAsync("everything you do", type: ActivityType.Watching);
            };
            _client.Log += LogAsync;
            _commands.Log += LogAsync;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            await _client.LoginAsync(TokenType.Bot, _config["token"]);
            await _client.StartAsync();

            await Task.Delay(-1, _cts.Token)
                .ContinueWith(async task =>
                {
                    await _client.StopAsync();
                    await _client.LogoutAsync();

                    _client.Dispose();
                });
        }

        private async Task LogAsync(LogMessage message)
        {
            if (message.Severity > LogSeverity.Verbose)
                return;
            try
            {
                await _colorLock.WaitAsync();
                ConsoleColor color = ConsoleColor.White;
                switch (message.Severity)
                {
                    case LogSeverity.Debug:
                        color = ConsoleColor.Gray;
                        break;
                    case LogSeverity.Verbose:
                        color = ConsoleColor.White;
                        break;
                    case LogSeverity.Info:
                        color = ConsoleColor.Green;
                        break;
                    case LogSeverity.Warning:
                        color = ConsoleColor.DarkYellow;
                        break;
                    case LogSeverity.Error:
                    case LogSeverity.Critical:
                        color = ConsoleColor.Red;
                        break;
                }
                Console.ForegroundColor = color;

                Console.WriteLine($"[{message.Severity.ToString().PadRight(7)}] {message.Source.PadRight(17)}@{DateTimeOffset.UtcNow.ToString("HH:mm:ss dd/mm")}" +
                    $"{message.Message}{(message.Exception != null ? Environment.NewLine : "")}{message.Exception?.Message ?? ""}");
            }
            finally
            {
                _colorLock.Release();
            }
        }

        private async Task HandleMessageReceived(SocketMessage rawMsg)
        {
            // Ignore system messages and messages from bots
            if (!(rawMsg is SocketUserMessage msg)) return;
            if (msg.Source != MessageSource.User) return;



            ICommandContext context;

            int argPos = 0;
            if (msg.HasStringPrefix(Global.adminPrefix, ref argPos))
            {
                context = new PrefixCommandContext(Global.adminPrefix, _client, msg);
            }
            else if (msg.HasStringPrefix(Global.userPrefix, ref argPos))
            {
                context = new PrefixCommandContext(Global.userPrefix, _client, msg);
            }
            else return;

            IResult result = await _commands.ExecuteAsync(context, argPos, _services);

            if (result.Error.HasValue &&
                result.Error.Value != CommandError.UnknownCommand)
                await context.Channel.SendMessageAsync(result.ToString());
        }

        private IConfiguration BuildConfig()
        {
            // Build and read a config file from the file system
#if DEBUG
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory() + "../../../config")
                .AddJsonFile("config.json")
                .Build();
#else
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory() + "/config")
                .AddJsonFile("config.json")
                .Build();
#endif
        }

        private IServiceProvider ConfigureServices()
            => new ServiceCollection()
                .AddSingleton(_client)
                .BuildServiceProvider();
    }
}
