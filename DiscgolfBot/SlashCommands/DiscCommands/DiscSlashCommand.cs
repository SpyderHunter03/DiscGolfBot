using DiscgolfBot.Data;
using DiscgolfBot.Data.Models;
using DiscgolfBot.Services;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscgolfBot.SlashCommands.DiscCommands
{
    public class DiscSlashCommand : ApplicationCommandModule
    {
        public IDiscRepository _discRespository { private get; set; } // The get accessor is optionally public, but the set accessor must be public.
        public IErrorService _errorService { private get; set; } // The get accessor is optionally public, but the set accessor must be public.

        [SlashCommand("disc", "Get disc information!")]
        public async Task Command(InteractionContext ctx, [Option("name", "Disc Name")] string discName)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent($"{ctx.Member.DisplayName} called /disc {discName}")
            );

            try
            {
                var disc = await _discRespository.GetDisc(discName);
                if (disc == null)
                {
                    await ctx.Channel.SendMessageAsync(GetFailedQueryEmbed(discName));
                    return;
                }

                var manufacturer = await _discRespository.GetManufacturer(disc.ManufacturerId);
                await ctx.Channel.SendMessageAsync(GetDiscEmbed(disc, manufacturer.Name));

                
            }
            catch (Exception ex)
            {
                await _errorService.CommandErrorThrown(ex, ctx, $"{ctx.Member.DisplayName} called /disc {discName}");
            }
        }

        protected static DiscordEmbed GetDiscEmbed(Disc disc, string manufacturer) =>
            new DiscordEmbedBuilder()
                    .WithTitle(disc.Name)
                    .WithDescription($"{manufacturer}\n{disc.Speed}, {disc.Glide}, {disc.Turn}, {disc.Fade}\n[PDGA](https://www.pdga.com/technical-standards/equipment-certification/discs/{disc.Name})")
                    .WithColor(DiscordColor.Azure)
                    .Build();

        protected static DiscordEmbed GetFailedQueryEmbed(string requestedDiscName) =>
            new DiscordEmbedBuilder()
                    .WithTitle("Unable to find disc")
                    .WithDescription($"We were unable to find a disc with the name of '{requestedDiscName}'.")
                    .WithColor(DiscordColor.Red)
                    .Build();
    }
}
