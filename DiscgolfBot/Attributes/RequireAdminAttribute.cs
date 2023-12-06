using DiscgolfBot.Data;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;

namespace DiscgolfBot.Attributes
{
    public class RequireAdminAttribute : SlashCheckBaseAttribute
    {
        public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx)
        {
            //return ctx.User.Id == 337045211362623493;
            var userRepository = ctx.Services.GetService<IUserRepository>();
            var isAdminUser = await userRepository!.IsAdminUser(ctx.User.Id);

            if (!isAdminUser)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent($"This command is for admins only.")
                );
            }

            return isAdminUser;
        }
    }
}
