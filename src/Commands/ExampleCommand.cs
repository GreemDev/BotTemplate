using System.Threading.Tasks;
using Voltemplate.Interactions;

namespace Voltemplate.Commands
{
    // There's *NO* need to register this anywhere! Inside InteractionService, commands are added from the current assembly.
    // That's completely changeable however; after all, this is a template.
    public class ExampleCommand : ApplicationCommand
    {
        public ExampleCommand() : base("echo", "Repeats the provided text back to you.")
            => Signature(o =>
            {
                o.RequiredString("text", "The text to repeat.");
            });

        public override async Task HandleSlashCommandAsync(SlashCommandContext ctx)
        {
            var reply = ctx.CreateReplyBuilder().WithEphemeral(); // ReplyBuilder<TInteraction>, used in all interactions for replying with a set of useful methods.
            var text = ctx.Options["text"].GetAsString(); // Option system similar to JDA's API.

            reply.WithContent(text);

            await reply.RespondAsync();
        }
    }
}