using DiscgolfBot.Data;
using DiscgolfBot.Data.Models.ViewModels;
using DiscgolfBot.Services;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscgolfBot.SlashCommands.BagCommands
{
    public class AdvancedBagSlashCommand : ApplicationCommandModule
    {
        public IBagRepository _bagRespository { private get; set; } // The get accessor is optionally public, but the set accessor must be public.
        public IErrorService _errorService { private get; set; } // The get accessor is optionally public, but the set accessor must be public.

        [SlashCommand("advbag", "Look at the advanced contents of a bag")]
        public async Task Command(InteractionContext ctx,
            [Option("user", "Bag Owner")] DiscordUser? user = null)
        {
            var userName = (user as DiscordMember)?.DisplayName ?? ctx.Member.DisplayName;

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent($"{ctx.Member.DisplayName} called /advbag {(user as DiscordMember)?.DisplayName ?? ""}")
            );

            try
            {
                //check to make sure the user is real
                var userId = user?.Id ?? ctx.Member.Id;

                // get bag with bag options => 
                var baggedDiscs = await _bagRespository.GetBaggedDiscs(userId);

                if (baggedDiscs == null || !baggedDiscs.Discs.Any())
                {
                    //No discs in the bag
                    await ctx.Channel.SendMessageAsync(NoDiscsInBagEmbed(userName, baggedDiscs));
                    return;
                }

                await ctx.Channel.SendMessageAsync(GetBaggedDiscsEmbed(userName, baggedDiscs));
                return;
            }
            catch (Exception ex)
            {
                await _errorService.CommandErrorThrown(ex, ctx, $"{ctx.Member.DisplayName} called /advbag {(user as DiscordMember)?.DisplayName ?? ""}");
            }
        }

        protected static DiscordEmbed GetBaggedDiscsEmbed(string userName, BaggedDiscs bag)
        {
            var embedBuilder = new DiscordEmbedBuilder()
                    .WithTitle($"{bag.BagName ?? "Main"}")
                    .WithDescription($"{userName}'s discs are:")
                    .WithColor(DiscordColor.Azure)
                    .WithFooter($"{userName} bags {bag.Discs.Count} molds");

            if (bag.BagPhoto != null)
                embedBuilder.WithThumbnail(bag.BagPhoto);

            if (bag.Putter != null)
                embedBuilder.AddField("Putting with:", $"{bag.Putter.Name} ({bag.Putter.FlightNumbers(", ")})");

            var putters = bag.Discs.Where(d => d.Speed < 4).ToList();
            if (putters.Count != 0)
                embedBuilder.AddField("Putter", string.Join("\n", putters.Select(d => $"{d.Name} ({d.FlightNumbers(", ")})")));

            var midranges = bag.Discs.Where(d => d.Speed >= 4 && d.Speed < 6).ToList();
            if (midranges.Count != 0)
                embedBuilder.AddField("Midrange", string.Join("\n", midranges.Select(d => $"{d.Name} ({d.FlightNumbers(", ")})")));

            var fairways = bag.Discs.Where(d => d.Speed >= 6 && d.Speed < 9).ToList();
            if (fairways.Count != 0)
                embedBuilder.AddField("Fairway", string.Join("\n", fairways.Select(d => $"{d.Name} ({d.FlightNumbers(", ")})")));

            var control = bag.Discs.Where(d => d.Speed >= 9 && d.Speed < 11).ToList();
            if (control.Count != 0)
                embedBuilder.AddField("Control", string.Join("\n", control.Select(d => $"{d.Name} ({d.FlightNumbers(", ")})")));

            var distance = bag.Discs.Where(d => d.Speed > 11).ToList();
            if (distance.Count != 0)
                embedBuilder.AddField("Distance", string.Join("\n", distance.Select(d => $"{d.Name} ({d.FlightNumbers(", ")})")));

            if (bag.Discs.Count != 0)
            {
                embedBuilder.AddField($"{userName}'s Bag Count by Brand", 
                    string.Join("\n", bag.Discs
                        .GroupBy(d => d.ManufacturerName)
                        .Select(m => new { ManufacturerName = m.Key, Count = m.Where(d => d.ManufacturerName == m.Key).Count() })
                        .OrderByDescending(m => m.Count)
                        .Select(m => $"{m.ManufacturerName} ({m.Count})")));
            }
                
            return embedBuilder.Build();
        }


        protected static DiscordEmbed NoDiscsInBagEmbed(string userName, BaggedDiscs? baggedDiscs) =>
            new DiscordEmbedBuilder()
                    .WithTitle($"{baggedDiscs?.BagName ?? $"{userName}'s bag"}")
                    .WithDescription($"{userName} doesn't have any discs in this bag")
                    .WithColor(DiscordColor.Orange)
                    .Build();
    }
}
