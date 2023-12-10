using DiscgolfBot.Data;
using DiscgolfBot.Data.Models;
using DiscgolfBot.Helpers;
using DiscgolfBot.Services;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Google.Protobuf.Collections;
using System.Collections.Generic;

namespace DiscgolfBot.SlashCommands.DiscCommands
{
    public class AdvancedDiscSlashCommand : ApplicationCommandModule
    {
        public IDiscRepository _discRespository { private get; set; } // The get accessor is optionally public, but the set accessor must be public.
        public IErrorService _errorService { private get; set; } // The get accessor is optionally public, but the set accessor must be public.

        [SlashCommand("advdisc", "Get advanced disc information!")]
        public async Task Command(InteractionContext ctx, [Option("name", "Disc Name")] string discName)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent($"{ctx.Member.DisplayName} called /advdisc {discName}")
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
                var discPictures = await _discRespository.GetDiscPictures(discName);
                var discPicture = discPictures != null && discPictures.Any() ? discPictures.ElementAt(new Random().Next(0, discPictures.Count())) : null;
                var discReviews = await _discRespository.GetDiscReviews(discName);
                var discCounts = (await _discRespository.GetDiscCounts())?.ToList();
                var count = discCounts?.FirstOrDefault(dc => dc.DiscName.ToLower().Equals(discName.ToLower()));

                //var popularity = discCounts?.IndexOf(count) + 1 ?? -1;
                var popularity = GetDiscPopularity(discCounts, discName);

                await ctx.Channel.SendMessageAsync(await GetDiscEmbed(ctx, disc, manufacturer.Name, discPicture, discReviews, count?.Count ?? 0, popularity?.Rank ?? -1, popularity?.DiscNames ?? [$"{disc.Name}"]));
            }
            catch (Exception ex)
            {
                await _errorService.CommandErrorThrown(ex, ctx, $"{ctx.Member.DisplayName} called /advdisc {discName}");
            }
        }

        protected static async Task<DiscordEmbed> GetDiscEmbed(InteractionContext ctx, Disc disc, string manufacturer, DiscPicture? discPicture, IEnumerable<DiscReview>? discReviews, int discCount, int popularityRank, IEnumerable<string> discsInPopularityRank)
        {
            var embed = new DiscordEmbedBuilder()
                    .WithTitle(disc.Name)
                    .WithDescription($"{manufacturer}\n{disc.Speed}, {disc.Glide}, {disc.Turn}, {disc.Fade}\n[PDGA](https://www.pdga.com/technical-standards/equipment-certification/discs/{disc.Name})")
                    .AddField("Number of times bagged", $"{discCount}")
                    .AddField("Global Popularity Rank", 
                        $"{(discsInPopularityRank.Count() > 1 ? 
                            $"T{popularityRank} ({string.Join(", ", discsInPopularityRank)})" : 
                            (popularityRank > 0 ? $"{popularityRank}" : "Not Bagged"))}")
                    .AddField("Technical Standards", $"Max Weight: {disc.MaxWeight?.ToString() ?? "Not Found"}\nDiameter: {disc.Diameter?.ToString() ?? "Not Found"}\nHeight: {disc.Height?.ToString() ?? "Not Found"}\n Depth: {disc.RimDepth?.ToString() ?? "Not Found"}\n Inside Rim Diameter: {disc.InsideRimDiameter?.ToString() ?? "Not Found"}\nRim Thickness: {disc.RimThickness?.ToString() ?? "Not Found"}\n Approved Date: {(disc.ApproveDate.HasValue ? $"{disc.ApproveDate.Value:d}" : "NotFound")}")
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
                embed.WithThumbnail(discPicture.Link)
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

        protected static (int Rank, List<string> DiscNames)? GetDiscPopularity(List<DiscCount>? discCount, string discName)
        {
            if (discCount == null || discCount.Count == 0)
                return null;

            int currentRank = 1;
            var groups = discCount.GroupBy(d => d.Count)
                              .OrderByDescending(g => g.Key);

            foreach (var group in groups)
            {
                List<string> discNames = group.Select(d => d.DiscName).ToList();
                if (group.Any(d => d.DiscName.Equals(discName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return (currentRank, discNames);
                }
                currentRank += group.Count();
            }

            // Return null if no disc with the given name is found
            return null;
        }
    }
}
