using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.Rest;
using Discord.WebSocket;
using Gommon;
using Humanizer;
using Voltemplate;
using Voltemplate.Interactions;
using Voltemplate.Services;
using Voltemplate.Entities;

namespace Voltemplate.Helpers
{
    public static class DiscordHelper
    {
        public static string Zws => "\u200B";
        public static string Wave => "\uD83D\uDC4B";
        public static string X => "\u274C";
        public static string BallotBoxWithCheck => "\u2611";
        public static string Clap => "\uD83D\uDC4F";
        public static string OkHand => "\uD83D\uDC4C";
        public static string One => "1️⃣";
        public static string Two => "2️⃣";
        public static string Three => "3️⃣";
        public static string Four => "4️⃣";
        public static string Five => "5️⃣";
        public static string Six => "6️⃣";
        public static string Seven => "7️⃣";
        public static string Eight => "8️⃣";
        public static string Nine => "9️⃣";
        public static string First => "⏮";
        public static string Left => "◀";
        public static string Right => "▶";
        public static string Last => "⏭";
        public static string WhiteSquare => "⏹";
        public static string OctagonalSign => "🛑";
        public static string E1234 => "🔢";
        public static string ArrowBackwards => "\u25C0";
        public static string SpaceInvader => "\uD83D\uDC7E";
        public static string Question => "\u2753";
        public static string Star => "\u2B50";

        public static Emoji[] GetPollEmojis()
            => new []
            {
                One.ToEmoji(), Two.ToEmoji(), Three.ToEmoji(), Four.ToEmoji(), Five.ToEmoji(),
                Six.ToEmoji(), Seven.ToEmoji(), Eight.ToEmoji(), Nine.ToEmoji()
            };

        public static RequestOptions CreateRequestOptions(Action<RequestOptions> initializer) 
            => new RequestOptions().Apply(initializer);

        public static string GetUrl(this Emoji emoji)
            =>
                $"https://i.kuro.mu/emoji/512x512/{emoji.ToString().GetUnicodePoints().Select(x => x.ToString("x2")).Join('-')}.png";


        /// <summary>
        ///     Checks if the current user is the user identified in the bot's config.
        /// </summary>
        /// <param name="user">The current user</param>
        /// <returns>True, if the current user is the bot's owner; false otherwise.</returns>
        public static bool IsBotOwner(this SocketUser user)
            => Config.Owner == user.Id;

        public static bool HasRole(this SocketGuildUser user, ulong roleId)
            => user.Roles.Select(x => x.Id).Contains(roleId);

        public static async Task<bool> TrySendMessageAsync(this SocketGuildUser user, string text = null,
            bool isTts = false, Embed embed = null, RequestOptions options = null)
        {
            try
            {
                await user.SendMessageAsync(text, isTts, embed, options);
                return true;
            }
            catch (HttpException)
            {
                return false;
            }
        }

        public static SocketRole GetHighestRole(this SocketGuildUser member, bool requireColor = true)
            => member?.Roles?.Where(x => !requireColor || x.HasColor())?
                .OrderByDescending(x => x.Position)?.FirstOrDefault();

        public static bool TryGetSpotifyStatus(this IGuildUser user, out SpotifyGame spotify)
        {
            spotify = user.Activities.FirstOrDefault(x => x is SpotifyGame).Cast<SpotifyGame>();
            return spotify != null;
        }

        internal static char GetTimestampFlagInternal(this TimestampType type) => (char)type;
        
        internal static string GetDiscordTimestampInternal(long unixSeconds, char timestampType)
            => $"<t:{unixSeconds}:{timestampType}>";
        
        public static string GetDiscordTimestamp(this DateTimeOffset dto, TimestampType type) =>
            GetDiscordTimestampInternal(dto.ToUnixTimeSeconds(), type.GetTimestampFlagInternal());

        public static string GetDiscordTimestamp(this DateTime date, TimestampType type) => 
            new DateTimeOffset(date).GetDiscordTimestamp(type);

        public static async Task<bool> TrySendMessageAsync(this SocketTextChannel channel, string text = null,
            bool isTts = false, Embed embed = null, RequestOptions options = null)
        {
            try
            {
                await channel.SendMessageAsync(text, isTts, embed, options);
                return true;
            }
            catch (HttpException)
            {
                return false;
            }
        }

        public static string GetInviteUrl(this DiscordSocketClient client, bool withAdmin = true)
            => withAdmin
                ? $"https://discord.com/oauth2/authorize?client_id={client.Rest.CurrentUser.Id}&scope=bot+applications.commands&permissions=8"
                : $"https://discord.com/oauth2/authorize?client_id={client.Rest.CurrentUser.Id}&scope=bot+applications.commands&permissions=402992246";

        public static SocketUser GetOwner(this BaseSocketClient client)
            => client.GetUser(Config.Owner);

        public static void RegisterEventHandlers(this DiscordSocketClient client, IServiceProvider provider)
        {
            client.Log += m => Task.Run(() => Logger.HandleLogEvent(new LogEventArgs(m)));

            client.Ready += async () =>
            {
                var guilds = client.Guilds.Count;
                var users = client.Guilds.SelectMany(x => x.Users).DistinctBy(x => x.Id).Count();
                var channels = client.Guilds.SelectMany(x => x.Channels).DistinctBy(x => x.Id).Count();

                Logger.PrintHeader();
                Logger.Info(LogSource.Bot, "Use this URL to invite me to your guilds:");
                Logger.Info(LogSource.Bot, $"{client.GetInviteUrl()}");
                Logger.Info(LogSource.Bot, $"Logged in as {client.CurrentUser}");
                Logger.Info(LogSource.Bot, "Connected to:");
                Logger.Info(LogSource.Bot, $"     {"guild".ToQuantity(guilds)}");
                Logger.Info(LogSource.Bot, $"     {"user".ToQuantity(users)}");
                Logger.Info(LogSource.Bot, $"     {"channel".ToQuantity(channels)}");

                var (type, name, streamer) = Config.ParseActivity();

                if (streamer is null && type != ActivityType.CustomStatus)
                {
                    await client.SetGameAsync(name, null, type);
                    Logger.Info(LogSource.Bot, $"Set {client.CurrentUser.Username}'s game to \"{Config.Game}\".");
                }
                else if (type != ActivityType.CustomStatus)
                {
                    await client.SetGameAsync(name, Config.FormattedStreamUrl, type);
                    Logger.Info(LogSource.Bot,
                        $"Set {client.CurrentUser.Username}'s activity to \"{type}: {name}\", at Twitch user {Config.Streamer}.");
                }
            };
        }

        public static Task<IUserMessage> SendToAsync(this EmbedBuilder e, IMessageChannel c) =>
            c.SendMessageAsync(embed: e.Build(), allowedMentions: AllowedMentions.None);

        public static Task<IUserMessage> SendToAsync(this Embed e, IMessageChannel c) =>
            c.SendMessageAsync(embed: e, allowedMentions: AllowedMentions.None);

        public static Task<IUserMessage> ReplyToAsync(this EmbedBuilder e, IUserMessage msg) =>
            msg.ReplyAsync(embed: e.Build(), allowedMentions: AllowedMentions.None);

        public static Task<IUserMessage> ReplyToAsync(this Embed e, IUserMessage msg) =>
            msg.ReplyAsync(embed: e, allowedMentions: AllowedMentions.None);


        // ReSharper disable twice UnusedMethodReturnValue.Global
        public static Task<IUserMessage> SendToAsync(this EmbedBuilder e, IUser u) => u.SendMessageAsync(embed: e.Build());

        public static Task<IUserMessage> SendToAsync(this Embed e, IUser u) => u.SendMessageAsync(embed: e);

        public static EmbedBuilder WithSuccessColor(this EmbedBuilder e) => e.WithColor(Config.SuccessColor);

        public static EmbedBuilder WithErrorColor(this EmbedBuilder e) => e.WithColor(Config.ErrorColor);

        public static EmbedBuilder WithRelevantColor(this EmbedBuilder e, SocketGuildUser user) =>
            e.WithColor(user.GetHighestRole()?.Color ?? new Color(Config.SuccessColor));

        public static EmbedBuilder AppendDescription(this EmbedBuilder e, string toAppend) =>
            e.WithDescription((e.Description ?? string.Empty) + toAppend);

        public static EmbedBuilder AppendDescriptionLine(this EmbedBuilder e, string toAppend = "") =>
            e.AppendDescription($"{toAppend}\n");

        public static Emoji ToEmoji(this string str) => new(str);

        public static bool ShouldHandle(this SocketMessage message, out SocketUserMessage userMessage)
        {
            if (message is SocketUserMessage msg && !msg.Author.IsBot)
            {
                userMessage = msg;
                return true;
            }

            userMessage = null;
            return false;
        }

        public static async Task<bool> TryDeleteAsync(this IDeletable deletable, RequestOptions options = null)
        {
            try
            {
                if (deletable is null) return false;
                await deletable.DeleteAsync(options);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static Task<bool> TryDeleteAsync(this IDeletable deletable, string reason)
            => deletable.TryDeleteAsync(CreateRequestOptions(opts => opts.AuditLogReason = reason));

        public static string GetEffectiveUsername(this IGuildUser user) =>
            user.Nickname ?? user.Username;

        public static string GetEffectiveAvatarUrl(this IUser user, ImageFormat format = ImageFormat.Auto,
            ushort size = 128)
            => user.GetAvatarUrl(format, size) ?? user.GetDefaultAvatarUrl();

        public static bool HasAttachments(this IMessage message)
            => !message.Attachments.IsEmpty();

        public static bool HasColor(this IRole role)
            => role.Color.RawValue != 0;

        public static EmbedBuilder WithDescription(this EmbedBuilder e, StringBuilder sb)
            => e.WithDescription(sb.ToString());
    }

    public enum TimestampType : sbyte
    {
        ShortTime = (sbyte)'t',
        LongTime = (sbyte)'T',
        ShortDate = (sbyte)'d',
        LongDate = (sbyte)'D',
        ShortDateTime = (sbyte)'f',
        LongDateTime = (sbyte)'F',
        Relative = (sbyte)'R'
    }
    
}