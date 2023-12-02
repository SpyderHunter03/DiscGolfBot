using DSharpPlus.SlashCommands;

namespace DiscgolfBot.Services
{
    public class ErrorService : IErrorService
    {
        public async Task CommandErrorThrown(Exception ex, InteractionContext ctx, string commandCalled)
        {
            await ctx.Channel.SendMessageAsync($"We are sorry, an error has occured. " +
                $"Please check back later for updates or change your command parameters.");

            Console.WriteLine($"{commandCalled}\n{ex}");

            var logsChannel = await ctx.Client.GetChannelAsync(1178678380816760965); //Spyder Web Server, #discgolfbotlogs channel
            await logsChannel.SendMessageAsync($"{ctx.Guild.Name}:{commandCalled}\n{ex}");
        }
    }
}
