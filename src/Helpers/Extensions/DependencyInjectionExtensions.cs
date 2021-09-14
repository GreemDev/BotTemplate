using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Discord;
using Discord.WebSocket;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Voltemplate;
using Voltemplate.Entities;
using Voltemplate.Helpers;
using Voltemplate.Services;

namespace Gommon
{
    public static partial class Extensions
    {
        public static IServiceCollection AddAllServices(this IServiceCollection coll) =>
            coll.AddBotServices()
                .AddSingleton<CancellationTokenSource>()
                .AddSingleton(new HttpClient
                {
                    Timeout = 10.Seconds()
                })
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    AlwaysAcknowledgeInteractions = false,
                    LogLevel = LogSeverity.Verbose,
                    GatewayIntents = Intents,
                    AlwaysDownloadUsers = false,
                    ConnectionTimeout = 10000,
                    MessageCacheSize = 0
                }));

        private static GatewayIntents Intents
            => GatewayIntents.Guilds;

        private static bool IsEligibleService(Type type) => type.Inherits<IBotService>() && !type.IsAbstract;

        public static IServiceCollection AddBotServices(this IServiceCollection serviceCollection)
            => serviceCollection.Apply(coll =>
            {
                //get all the classes that inherit IVanslateService, and aren't abstract.
                var l = typeof(Bot).Assembly.GetTypes()
                    .Where(IsEligibleService).Apply(ls => ls.ForEach(coll.TryAddSingleton));
                Logger.Info(LogSource.Bot, $"Injected {"service".ToQuantity(l.Count())} into the service provider.");
            });
    }
}