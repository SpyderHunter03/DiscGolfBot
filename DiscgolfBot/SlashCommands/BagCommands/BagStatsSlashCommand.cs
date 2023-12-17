using DiscgolfBot.Data;
using DiscgolfBot.Services;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscgolfBot.SlashCommands.BagCommands
{
    public class BagStatsSlashCommand : ApplicationCommandModule
    {
        public IBagRepository _bagRespository { private get; set; } // The get accessor is optionally public, but the set accessor must be public.
        public IErrorService _errorService { private get; set; } // The get accessor is optionally public, but the set accessor must be public.

        [SlashCommand("bagstats", "Look at the advanced bag statistics")]
        public async Task Command(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent($"{ctx.Member.DisplayName} called /bagstats")
            );

            try
            {
                var bags = await _bagRespository.GetBaggedDiscs();

                if (bags == null || !bags.Any())
                {
                    //No bags
                    await ctx.Channel.SendMessageAsync(NoBagsEmbed());
                    return;
                }

                var totalNumOfBags = bags.Count();
                var avgMoldsPerBag = bags.Where(b => b.Discs.Any()).Average(b => b.Discs.Count);
                var avgBrandsPerBag = bags.Where(b => b.Discs.Any()).Average(b => b.Discs.Select(d => d.ManufacturerId).Distinct().Count());
                var topPuttingPutters = bags
                    .Where(p => p.Putter != null)
                    .Select(b => b.Putter!.Name)
                    .GroupBy(p => p)
                    .Select(n => new TopItems { Name = n.Key, Count = n.Count() })
                    .OrderByDescending(c => c.Count)
                    .Take(25).ToList();
                var topMolds = bags
                    .Where(b => b.Discs.Any())
                    .SelectMany(b => b.Discs)
                    .GroupBy(d => d.Name)
                    .Select(n => new TopItems { Name = n.Key, Count = n.Count() })
                    .OrderByDescending(c => c.Count)
                    .Take(25).ToList();
                var topBrands = bags
                    .Where(b => b.Discs.Any())
                    .SelectMany(b => b.Discs)
                    .GroupBy(d => d.ManufacturerName)
                    .Select(n => new TopItems { Name = n.Key, Count = n.Count() })
                    .OrderByDescending(c => c.Count)
                    .Take(25).ToList();
                var topBrandAmbassadors = bags
                    .Where(b => b.Discs.Any())
                    .Select(b => new BrandAmbassador
                    {
                        User = b.UserId,
                        TopBrand = b.Discs
                            .GroupBy(d => d.ManufacturerName)
                            .Select(m => new Brand { ManufacturerName = m.Key, Count = m.Count() })
                            .OrderByDescending(m => m.Count)
                            .First()
                    })
                    .OrderByDescending(u => u.TopBrand.Count)
                    .Take(10).ToList();
                foreach (var brandAmbassador in topBrandAmbassadors)
                {
                    var user = await ctx.Client.GetUserAsync(brandAmbassador.User);
                    brandAmbassador.UserName = user.Username;
                }

                
                var topBrandDiverseness = bags
                    .Where(b => b.Discs.Any())
                    .Select(b => new BrandDiverseness
                    {
                        User = b.UserId,
                        BrandCount = b.Discs.Select(d => d.ManufacturerId).Distinct().Count()
                    })
                    .OrderByDescending(u => u.BrandCount)
                    .Take(10).ToList();
                foreach (var brandDiverseness in topBrandDiverseness)
                {
                    var user = await ctx.Client.GetUserAsync(brandDiverseness.User);
                    brandDiverseness.UserName = user.Username;
                }

                await ctx.Channel.SendMessageAsync(GetBagStatsEmbed(totalNumOfBags, avgMoldsPerBag, avgBrandsPerBag, topPuttingPutters, topMolds, topBrands, topBrandAmbassadors, topBrandDiverseness));
                return;
            }
            catch (Exception ex)
            {
                await _errorService.CommandErrorThrown(ex, ctx, $"{ctx.Member.DisplayName} called /bagstats");
            }
        }

        private static DiscordEmbed GetBagStatsEmbed(
            int totalNumOfBags, 
            double avgMoldsPerBag, 
            double avgBrandsPerBag, 
            IEnumerable<TopItems> topPuttingPutters,
            IEnumerable<TopItems> topMolds,
            IEnumerable<TopItems> topBrands,
            IEnumerable<BrandAmbassador> topBrandAmbassadors,
            IEnumerable<BrandDiverseness> topBrandDiverseness
        )
        {
            return new DiscordEmbedBuilder()
                .WithTitle($"Discord Bag Stats")
                .WithColor(DiscordColor.Azure)
                .AddField("Total # of Bags:", $"{totalNumOfBags}", true)
                .AddField("Avg Molds per Bag:", $"{avgMoldsPerBag}", true)
                .AddField("Avg Brands per Bag:", $"{avgBrandsPerBag}", true)
                .AddField($"Top {topPuttingPutters.Count()} Putting Putters:", $"{string.Join(", ", topPuttingPutters.Select(p => $"{p.Name} ({p.Count})"))}")
                .AddField($"Top {topMolds.Count()} Molds:", $"{string.Join(", ", topMolds.Select(p => $"{p.Name} ({p.Count})"))}")
                .AddField($"Top {topBrands.Count()} Brands:", $"{string.Join(", ", topBrands.Select(p => $"{p.Name} ({p.Count})"))}")
                .AddField($"Top {topBrandAmbassadors.Count()} Brand Ambassadors:", $"{string.Join(", ", topBrandAmbassadors.Select(p => $"{p.TopBrand.ManufacturerName} ({p.TopBrand.Count}): {p.UserName}"))}", true)
                .AddField($"Top {topBrandDiverseness.Count()} Diverse Bags by Number of Brands:", $"{string.Join(", ", topBrandDiverseness.Select(p => $"({p.BrandCount}): {p.UserName} "))}", true)
                .Build();
        }


        protected static DiscordEmbed NoBagsEmbed() =>
            new DiscordEmbedBuilder()
                    .WithTitle($"No Bags")
                    .WithDescription($"There are no bags to figure out the stats on.")
                    .WithColor(DiscordColor.Orange)
                    .Build();

        private class TopItems
        {
            public string Name { get; set; } = string.Empty;
            public int Count { get; set; }
        }

        private class BrandAmbassador
        {
            public ulong User { get; set; }
            public string UserName { get; set; } = string.Empty;
            public Brand TopBrand { get; set; } = new();
        }

        private class Brand
        {
            public string ManufacturerName { get; set; } = string.Empty;
            public int Count { get; set; }
        }

        private class BrandDiverseness
        {
            public ulong User { get; set; }
            public string UserName { get; set; } = string.Empty;
            public int BrandCount { get; set; }
        }
    }
}
