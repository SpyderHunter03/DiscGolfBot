using DiscgolfBot.Data;
using DiscgolfBot.Data.Models;
using DiscgolfBot.Helpers;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscgolfBot.SlashCommands.DiscCommands
{
    public class AdvancedDiscSlashCommand : ApplicationCommandModule
    {
        public IDiscRepository _discRespository { private get; set; } // The get accessor is optionally public, but the set accessor must be public.

        [SlashCommand("advdisc", "Get advanced disc information!")]
        public async Task Command(InteractionContext ctx, [Option("name", "Disc Name")] string discName)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent($"{ctx.Member.DisplayName} called /advdisc {discName}")
            );

            try
            {
                var disc = await _discRespository.GetAdvancedDisc(discName);
                if (disc == null)
                {
                    await ctx.Channel.SendMessageAsync(GetFailedQueryEmbed(discName));
                    return;
                }

                var discPictures = await _discRespository.GetDiscPictures(discName);
                var discPicture = discPictures != null && discPictures.Any() ? discPictures.ElementAt(new Random().Next(0, discPictures.Count())) : null;
                var discReviews = await _discRespository.GetDiscReviews(discName);
                var discCounts = await _discRespository.GetDiscCounts();
                var count = discCounts?.FirstOrDefault(dc => dc.DiscName.ToLower().Equals(discName));
                var popularity = (discCounts as List<DiscCount>)?.IndexOf(count) + 1 ?? -1;

                await ctx.Channel.SendMessageAsync(await GetDiscEmbed(ctx, disc, discPicture, discReviews, count?.Count ?? 0, popularity));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ctx.Member.DisplayName} called /advdisc {discName} \n{ex}");
                await ctx.Channel.SendMessageAsync($"An error has occured. Check logs.");
            }
        }

        protected static async Task<DiscordEmbed> GetDiscEmbed(InteractionContext ctx, AdvancedDisc disc, DiscPicture? discPicture, IEnumerable<DiscReview>? discReviews, int discCount, int popularity)
        {
            var embed = new DiscordEmbedBuilder()
                    .WithTitle(disc.Name)
                    .WithDescription($"{disc.Manufacturer}\n{disc.Speed}, {disc.Glide}, {disc.Turn}, {disc.Fade}\n[PDGA](https://www.pdga.com/technical-standards/equipment-certification/discs/{disc.Name})")
                    .AddField("Number of times bagged", $"{discCount}")
                    .AddField("Global Popularity Rank", $"{popularity}")
                    .AddField("Technical Standards", $"Max Weight: {disc.MaxWeight}\nDiameter: {disc.Diameter}\nHeight: {disc.Height}\n Depth: {disc.RimDepth}\n Inside Rim Diameter: {disc.InsideRimDiameter}\nRim Thickness: {disc.RimThickness}\n Approved Date: {disc.ApproveDate.Date:d}")
                    .WithColor(DiscordColor.Azure);

            if (discReviews != null && discReviews.Any())
            {
                foreach (var discReview in discReviews.Take(25))
                {
                    var user = await discReview.UserId.GetUser(ctx);
                    embed.AddField($"Review by {user.Username}", $"{discReview.Review.GetStringFromBlob()}");
                }
            }

            if (discPicture != null)
            {
                var user = await discPicture.UserId.GetUser(ctx);
                embed.WithImageUrl(discPicture.Link)
                    .WithFooter($"Photo submitted by: {user.Username}");
            }

            return embed.Build();
        }


        protected static DiscordEmbed GetFailedQueryEmbed(string requestedDiscName) =>
            new DiscordEmbedBuilder()
                    .WithTitle("Unable to find disc")
                    .WithDescription($"We were unable to find a disc with the name of '{requestedDiscName}'.")
                    .WithColor(DiscordColor.Red)
                    .Build();
    }
}
