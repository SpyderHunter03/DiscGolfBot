using DSharpPlus.SlashCommands;

namespace DiscgolfBot.Services
{
    public interface IErrorService
    {
        Task CommandErrorThrown(Exception ex, InteractionContext ctx, string commandCalled);
    }
}
