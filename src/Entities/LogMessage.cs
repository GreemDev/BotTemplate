using System;
using Discord;

namespace Voltemplate.Entities
{
    public record LogMessage
    {
        public LogSeverity Severity { get; init; }
        public LogSource Source { get; init; }
        public string Message { get; init; }
        public Exception Exception { get; init; }

        public static LogMessage FromDiscordLogMessage(Discord.LogMessage message)
            => new()
            {
                Message = message.Message,
                Severity = message.Severity,
                Exception = message.Exception,
                Source = LogSources.Parse(message.Source)
            };
    }
}