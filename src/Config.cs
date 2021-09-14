using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Discord;
using Gommon;
using Voltemplate.Entities;
using Voltemplate.Helpers;

namespace Voltemplate
{
    public static class Config
    {
        public const string DataDirectory = "data";
        public const string ConfigFilePath = DataDirectory + "/config.json";
        private static BotConfig _configuration;

        public static readonly JsonSerializerOptions JsonOptions = new()
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            IgnoreNullValues = false,
            AllowTrailingCommas = true
        };

        private static bool IsValidConfig() 
            => File.Exists(ConfigFilePath) && !File.ReadAllText(ConfigFilePath).IsNullOrEmpty();

        public static bool StartupChecks()
        {
            if (!Directory.Exists(DataDirectory))
            {
                Logger.Error(LogSource.Bot,
                    $"The \"{DataDirectory}\" directory didn't exist, so I created it for you. Please fill in the configuration!");
                Directory.CreateDirectory(DataDirectory);
                //99.9999999999% of the time the config also won't exist if this block is reached
                //if the config does exist when this block is reached, feel free to become the lead developer of this project
            }

            if (CreateIfAbsent()) return true;
            Logger.Error(LogSource.Bot,
                $"Please fill in the configuration located at \"{ConfigFilePath}\"; restart me when you've done so.");
            return false;

        }
        
        public static bool CreateIfAbsent()
        {
            if (IsValidConfig()) return true;
            _configuration = new BotConfig
            {
                Token = "token here",
                Owner = 0,
                Game = "game here",
                Streamer = "streamer here",
                EnableDebugLogging = false,
                SuccessEmbedColor = 0x7000FB,
                ErrorEmbedColor = 0xFF0000,
            };
            try
            {
                File.WriteAllText(ConfigFilePath, JsonSerializer.Serialize(_configuration, JsonOptions));
            }
            catch (Exception e)
            {
                Logger.Error(LogSource.Bot, e.Message, e);
            }

            return false;
        }

        public static void Load()
        {
            _ = CreateIfAbsent();
            if (IsValidConfig())
                _configuration = JsonSerializer.Deserialize<BotConfig>(File.ReadAllText(ConfigFilePath), JsonOptions);                    
        }

        public static bool Reload()
        {
            try
            {
                _configuration = JsonSerializer.Deserialize<BotConfig>(File.ReadAllText(ConfigFilePath));
                return true;
            }
            catch (JsonException e)
            {
                Logger.Exception(e);
                return false;
            }
        }

        public static (ActivityType Type, string Name, string Streamer) ParseActivity()
        {
            var split = Game.Split(" ");
            var title = split.Skip(1).Join(" ");
            if (split[0].ToLower() is "streaming") title = split.Skip(2).Join(" ");
            return split.First().ToLower() switch
            {
                "playing" => (ActivityType.Playing, title, null),
                "listeningto" => (ActivityType.Listening, title, null),
                "listening" => (ActivityType.Listening, title, null),
                "streaming" => (ActivityType.Streaming, title, split[1]),
                "watching" => (ActivityType.Watching, title, null),
                _ => (ActivityType.Playing, Game, null)
            };
        }

        public static bool IsValidToken() 
            => !(Token.IsNullOrEmpty() || Token.Equals("token here"));

        public static string Token => _configuration.Token;

        public static ulong Owner => _configuration.Owner;

        public static string Game => _configuration.Game;

        public static string Streamer => _configuration.Streamer;

        public static bool EnableDebugLogging => _configuration.EnableDebugLogging;

        public static string FormattedStreamUrl => $"https://twitch.tv/{Streamer}";

        public static uint SuccessColor => _configuration.SuccessEmbedColor;

        public static uint ErrorColor => _configuration.ErrorEmbedColor;
        
        // ReSharper disable MemberHidesStaticFromOuterClass
        public record BotConfig
        {
            [JsonPropertyName("discord_token")]
            public string Token { get; init; }

            [JsonPropertyName("bot_owner")]
            public ulong Owner { get; init; }

            [JsonPropertyName("status_game")]
            public string Game { get; init; }

            [JsonPropertyName("status_twitch_streamer")]
            public string Streamer { get; init; }

            [JsonPropertyName("enable_debug_logging")]
            public bool EnableDebugLogging { get; init; }

            [JsonPropertyName("color_success")]
            public uint SuccessEmbedColor { get; init; }

            [JsonPropertyName("color_error")]
            public uint ErrorEmbedColor { get; init; }
        }
    }
}