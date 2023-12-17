using DiscgolfBot.Data;
using DiscgolfBot.Data.Models;
using DiscgolfBot.Services;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscgolfBot.SlashCommands.GeneralCommands
{
    public class AdminsSlashCommand : ApplicationCommandModule
    {
        public IAdminRepository _adminRespository { private get; set; } // The get accessor is optionally public, but the set accessor must be public.
        public IErrorService _errorService { private get; set; } // The get accessor is optionally public, but the set accessor must be public.

        [SlashCommand("admins", "Get admin names")]
        public async Task Command(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent($"{ctx.Member.DisplayName} called /admins")
            );

            try
            {
                var admins = await _adminRespository.GetAdmins();
                var adminDiscordUser = new List<DiscordUser>();
                foreach(var admin in admins )
                {
                    var user = await ctx.Client.GetUserAsync(admin.UserId);

                    if (ctx.Guild.Members.TryGetValue(user.Id, out var member))
                    {
                        adminDiscordUser.Add(member);
                    } else
                    {
                        adminDiscordUser.Add(user);
                    }

                }
                var adminNames = adminDiscordUser.Select(a => (a as DiscordMember)?.DisplayName ?? a.Username);
                await ctx.Channel.SendMessageAsync(GetAdminsEmbed(adminNames));
            }
            catch (Exception ex)
            {
                await _errorService.CommandErrorThrown(ex, ctx, $"{ctx.Member.DisplayName} called /admins");
            }
        }

        protected static DiscordEmbed GetAdminsEmbed(IEnumerable<string> admins) =>
            new DiscordEmbedBuilder()
                    .WithTitle($"List of Admins")
                    .WithDescription($"{string.Join("\n", admins)}")
                    .WithColor(DiscordColor.Azure)
                    .Build();
    }
}
