using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.Rest;
using Discord.WebSocket;
using Gommon;
using Humanizer;
using Voltemplate.Entities;
using Voltemplate.Helpers;
using Voltemplate.Services;

namespace Voltemplate.Interactions
{
    public class CommandUpdatingService
    {
        private readonly InteractionService _interaction;
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _provider;
        private bool _forcedUpdateAllCommands;
        public static bool ForceUpdateAllCommands { get; set; }

        public CommandUpdatingService(InteractionService interactionService,
            IServiceProvider provider)
        {
            _interaction = interactionService;
            _client = provider.Get<DiscordSocketClient>();
            _provider = provider;
        }

        public Task InitAsync()
        {
            _client.Ready += async () =>
            {
                if (!_forcedUpdateAllCommands && ForceUpdateAllCommands)
                {
                    try
                    {
                        var regularCommands = await UpsertRegularCommandsAsync();

                        var commandStr = $"{"Global command".ToQuantity(regularCommands.Count)}: ";

                        Logger.Info(LogSource.Rest,
                            commandStr + regularCommands.Select(x => x.Name).ToReadableString());

                        _forcedUpdateAllCommands = true;
                    }
                    catch (ApplicationCommandException e)
                    {
                        Logger.Debug(LogSource.Rest,
                            JsonSerializer.Serialize(JsonSerializer.Deserialize<object>(e.RequestJson),
                                Config.JsonOptions));
                    }
                }
            };

            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<RestGlobalCommand>> UpsertRegularCommandsAsync()
            => _client.Rest.BulkOverwriteGlobalCommands(
                _interaction.AllRegisteredCommands.GetCommandBuilders(_provider));
    }
}