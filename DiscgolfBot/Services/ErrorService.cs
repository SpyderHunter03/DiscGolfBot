using DSharpPlus.SlashCommands;

namespace DiscgolfBot.Services
{
    public class ErrorService : IErrorService
    {
        public async Task CommandErrorThrown(Exception ex, InteractionContext ctx, string commandCalled)
        {
            await ctx.Channel.SendMessageAsync($"An error has occured. Check logs.");
            Console.WriteLine($"{commandCalled}\n{ex}");
            var logsChannel = await ctx.Client.GetChannelAsync(1178678380816760965);
            await logsChannel.SendMessageAsync($"{ctx.Guild.Name}:{commandCalled}\n{ex}");
        }
    }
}
