using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Gommon;
using Microsoft.Extensions.DependencyInjection;
using Voltemplate.Entities;
using Voltemplate.Helpers;
using Voltemplate.Interactions;
using Voltemplate.Services;
using Console = Colorful.Console;


namespace Voltemplate
{
    public class Bot
    {
        public const string Name = "Discord Bot";
        
        internal static async Task Main(string[] args)
        {
#if DEBUG
            if (args.ContainsIgnoreCase("--force-command-update")) 
                CommandUpdatingService.ForceUpdateAllCommands = true;
#endif
            
            await StartAsync();
        }
        
        public static Task StartAsync()
        {
            Console.Title = Name;
            Console.CursorVisible = false;
            return new Bot().LoginAsync();
        }

        private IServiceProvider _provider;
        private DiscordSocketClient _client;
        private CancellationTokenSource _cts;

        private static IServiceProvider BuildServiceProvider()
            => new ServiceCollection()
                .AddAllServices()
                .BuildServiceProvider();

        private Bot()
            => Console.CancelKeyPress += (_, __) => _cts?.Cancel();

        private async Task LoginAsync()
        {
            if (!Config.StartupChecks()) return;

            Config.Load();

            if (!Config.IsValidToken()) return;

            _provider = BuildServiceProvider();
            _client = _provider.Get<DiscordSocketClient>();
            _cts = _provider.Get<CancellationTokenSource>();

            await _client.LoginAsync(TokenType.Bot, Config.Token);
            await _client.StartAsync();
            
            _client.RegisterEventHandlers(_provider);

            await _provider.Get<InteractionService>().InitAsync();

            try
            {
                await Task.Delay(-1, _cts.Token);
            }
            catch
            {
                await ShutdownAsync(_client, _provider);
            }
        }

        // ReSharper disable SuggestBaseTypeForParameter
        public static async Task ShutdownAsync(DiscordSocketClient client, IServiceProvider provider)
        {
            Logger.Critical(LogSource.Bot,
                "Bot shutdown requested; shutting down and cleaning up.");

            foreach (var disposable in provider.GetServices<IDisposable>())
                disposable?.Dispose();

            await client.SetStatusAsync(UserStatus.Invisible);
            await client.LogoutAsync();
            await client.StopAsync();
            Environment.Exit(0);
        }
    }
}