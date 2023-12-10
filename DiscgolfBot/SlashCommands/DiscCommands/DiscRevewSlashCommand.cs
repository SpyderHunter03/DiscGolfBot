using DiscgolfBot.Data;
using DiscgolfBot.Data.Models;
using DiscgolfBot.Helpers;
using DiscgolfBot.Services;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscgolfBot.SlashCommands.DiscCommands
{
    public class DiscReviewSlashCommand : ApplicationCommandModule
    {
        public IDiscRepository _discRespository { private get; set; } // The get accessor is optionally public, but the set accessor must be public.
        public IErrorService _errorService { private get; set; } // The get accessor is optionally public, but the set accessor must be public.

        [SlashCommand("reviewdisc", "Review a disc")]
        public async Task Command(InteractionContext ctx, [Option("name", "Disc Name")] string discName, [Option("review", "Disc Review")] string discReview)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent($"{ctx.Member.DisplayName} called /reviewdisc {discName} {discReview}")
            );

            try
            {
                var savedReview = discReview.GetBlobFromString();

                var embed = new DiscordEmbedBuilder()
                    .WithTitle("Blah")
                    .WithDescription($"Blah Blah.")
                    .WithColor(DiscordColor.Red)
                    .Build();

                await ctx.Channel.SendMessageAsync(embed);
            }
            catch (Exception ex)
            {
                await _errorService.CommandErrorThrown(ex, ctx, $"{ctx.Member.DisplayName} called /reviewdisc {discName} {discReview}");
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
