using DiscgolfBot.Data.Models.ViewModels;
using DiscgolfBot.Data;
using DiscgolfBot.Services;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus;
using DiscgolfBot.SlashCommands.ChoiceProviders;

namespace DiscgolfBot.SlashCommands.DiscCommands
{
    public class BrandStatsSlashCommand : ApplicationCommandModule
    {
        public IDiscRepository _discRespository { private get; set; } // The get accessor is optionally public, but the set accessor must be public.
        public IBagRepository _bagRespository { private get; set; } // The get accessor is optionally public, but the set accessor must be public.
        public IErrorService _errorService { private get; set; } // The get accessor is optionally public, but the set accessor must be public.

        [SlashCommand("brandstats", "Look at the advanced brand statistics")]
        public async Task Command(InteractionContext ctx, [Autocomplete(typeof(ManufacturerChoiceProvider))][Option("manufacturer", "Manufacturer", true)] string manufacturer)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent($"{ctx.Member.DisplayName} called /brandstats {manufacturer}")
            );

            try
            {

                var dbManufacturer = await _discRespository.GetManufacturer(manufacturer);
                if (dbManufacturer == null)
                {
                    //Manufacturer not found
                    await ctx.Channel.SendMessageAsync(NoManufacturerFound(manufacturer));
                    return;
                }

                var bags = await _bagRespository.GetBaggedDiscs();
                if (bags == null || !bags.Any())
                {
                    //No bags
                    await ctx.Channel.SendMessageAsync(NoBagsEmbed());
                    return;
                }

                var bagsHoldingManufacturer = bags.Where(b => 
                    b.Discs.Any(d => d.ManufacturerName.Equals(dbManufacturer.Name)) || 
                    (b.Putter?.ManufacturerName.Equals(dbManufacturer.Name) ?? false));
                if (bagsHoldingManufacturer == null || !bagsHoldingManufacturer.Any())
                {
                    //No Bags With Manufacturer
                    await ctx.Channel.SendMessageAsync(NoBagsWithManufacturerEmbed(dbManufacturer.Name));
                    return;
                }

                var totalNumOfBags = bagsHoldingManufacturer.Count();
                var topPuttingPutters = bagsHoldingManufacturer
                    .Where(p => p.Putter != null && p.Putter.ManufacturerName.Equals(dbManufacturer.Name))
                    .Select(b => b.Putter!.Name)
                    .GroupBy(p => p)
                    .Select(n => new TopItems { Name = n.Key, Count = n.Count() })
                    .OrderByDescending(c => c.Count)
                    .Take(25).ToList();
                var topMolds = bagsHoldingManufacturer
                    .Where(b => b.Discs.Any())
                    .SelectMany(b => b.Discs)
                    .Where(d => d.ManufacturerName.Equals(dbManufacturer.Name))
                    .GroupBy(d => d.Name)
                    .Select(n => new TopItems { Name = n.Key, Count = n.Count() })
                    .OrderByDescending(c => c.Count)
                    .Take(25).ToList();
                var topAmbassadors = bagsHoldingManufacturer
                    .Where(b => b.Discs.Any())
                    .Select(b => new Ambassador
                    {
                        User = b.UserId,
                        Count = b.Discs
                            .Where(d => d.ManufacturerName.Equals(dbManufacturer.Name))
                            .Count()
                    })
                    .OrderByDescending(u => u.Count)
                    .Take(20).ToList();
                foreach (var ambassador in topAmbassadors)
                {
                    var user = await ctx.Client.GetUserAsync(ambassador.User);
                    ambassador.UserName = user.Username;
                }

                await ctx.Channel.SendMessageAsync(GetBagStatsEmbed(dbManufacturer.Name, totalNumOfBags, topPuttingPutters, topMolds, topAmbassadors));
                return;
            }
            catch (Exception ex)
            {
                await _errorService.CommandErrorThrown(ex, ctx, $"{ctx.Member.DisplayName} called /brandstats {manufacturer}");
            }
        }

        private static DiscordEmbed GetBagStatsEmbed(
            string manufacturerName,
            int totalNumOfBags,
            IEnumerable<TopItems> topPuttingPutters,
            IEnumerable<TopItems> topMolds,
            IEnumerable<Ambassador> topBrandAmbassadors
        )
        {
            return new DiscordEmbedBuilder()
                .WithTitle($"Discord Bag Stats")
                .WithColor(DiscordColor.Azure)
                .AddField($"Total # of Bags with {manufacturerName}:", $"{totalNumOfBags}")
                .AddField($"Top {topPuttingPutters.Count()} Putting Putters:", $"{string.Join(", ", topPuttingPutters.Select(p => $"{p.Name} ({p.Count})"))}")
                .AddField($"Top {topMolds.Count()} Molds:", $"{string.Join(", ", topMolds.Select(p => $"{p.Name} ({p.Count})"))}")
                .AddField($"Top {topBrandAmbassadors.Count()} {manufacturerName} Ambassadors:", $"{string.Join(", ", topBrandAmbassadors.Select(p => $"{manufacturerName} ({p.Count}): {p.UserName}"))}")
                .Build();
        }

        protected static DiscordEmbed NoManufacturerFound(string manufacturer) =>
            new DiscordEmbedBuilder()
                    .WithTitle($"Manufacturer Not Found")
                    .WithDescription($"The manufacturer {manufacturer} is not in the database.")
                    .WithColor(DiscordColor.Red)
                    .Build();

        protected static DiscordEmbed NoBagsEmbed() =>
            new DiscordEmbedBuilder()
                    .WithTitle($"No Bags")
                    .WithDescription($"There are no bags to figure out the stats on.")
                    .WithColor(DiscordColor.Orange)
                    .Build();

        protected static DiscordEmbed NoBagsWithManufacturerEmbed(string manufacturer) =>
            new DiscordEmbedBuilder()
                    .WithTitle($"No Bags With Manufacturer")
                    .WithDescription($"There are no bags that have any discs from manufacturer {manufacturer}.")
                    .WithColor(DiscordColor.Yellow)
                    .Build();

        private class TopItems
        {
            public string Name { get; set; } = string.Empty;
            public int Count { get; set; }
        }

        private class Ambassador
        {
            public ulong User { get; set; }
            public string UserName { get; set; } = string.Empty;
            public int Count { get; set; }
        }
    }
}
