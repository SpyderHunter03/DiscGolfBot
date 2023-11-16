using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext;

namespace DiscgolfBot.Commands
{
    public class DiscCommand : BaseCommandModule
    {
        [Command("disc")]
        public async Task Command(CommandContext ctx)
        {
            Console.WriteLine("Greet Called");
            await ctx.Channel.SendMessageAsync("Greetings! Thank you for executing me!");
        }
    }
}
