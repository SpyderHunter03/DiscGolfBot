using DiscgolfBot.Data;
using DiscgolfBot.Data.Models;
using DiscgolfBot.Data.Models.ViewModels;
using DiscgolfBot.Services;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscgolfBot.SlashCommands.BagCommands
{
    public class MyBagSlashCommand : ApplicationCommandModule
    {
        public IDiscRepository _discRespository { private get; set; } // The get accessor is optionally public, but the set accessor must be public.
        public IBagRepository _bagRespository { private get; set; } // The get accessor is optionally public, but the set accessor must be public.
        public IErrorService _errorService { private get; set; } // The get accessor is optionally public, but the set accessor must be public.

        [SlashCommand("mybag", "View your bag with specific discs for your molds")]
        public async Task Command(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent($"{ctx.Member.DisplayName} called /mybag")
            );

            try
            {
                var userName = ctx.Member.DisplayName;
                var userId = ctx.Member.Id;

                var myBag = await _bagRespository.GetMyBag(userId);
                if (myBag == null || !myBag.Discs.Any())
                {
                    //No discs in the bag
                    await ctx.Channel.SendMessageAsync(NoDiscsInBagEmbed(userName, myBag));
                    return;
                }

                await ctx.Channel.SendMessageAsync(GetMyBagEmbed(userName, myBag));
                return;
            }
            catch (Exception ex)
            {
                await _errorService.CommandErrorThrown(ex, ctx, $"{ctx.Member.DisplayName} called /mybag");
            }
        }

        protected static DiscordEmbed NoDiscsInBagEmbed(string userName, BaggedDiscs? baggedDiscs) =>
            new DiscordEmbedBuilder()
                    .WithTitle($"{baggedDiscs?.BagName ?? $"{userName}'s bag"}")
                    .WithDescription($"{userName} doesn't have any discs in this bag")
                    .WithColor(DiscordColor.Orange)
                    .Build();

        protected static DiscordEmbed GetMyBagEmbed(string userName, BaggedDiscs bag)
        {
            var embedBuilder = new DiscordEmbedBuilder()
                    .WithTitle($"{bag.BagName ?? "Main"}")
                    .WithDescription($"{userName}'s discs are:")
                    .WithColor(DiscordColor.Azure)
                    .WithFooter($"{userName} bags {bag.Discs.Count} molds");

            if (bag.BagPhoto != null)
                embedBuilder.WithThumbnail(bag.BagPhoto);

            if (bag.Putter != null)
                embedBuilder.AddField("Putting with:", bag.Putter.Name);

            var putters = bag.Discs.Where(d => d.Speed < 4).ToList();
            if (putters.Any())
                embedBuilder.AddField("Putter", string.Join("\n", putters.Select(d => $"{d.Name}{(((d as MyDiscs).Discs?.Any() ?? false) ? $"\n{string.Join("\n", (d as MyDiscs).Discs.Select(myd => $"- {myd.Description}, {myd.Plastic}, {myd.Weight}g, {myd.FlightNumbers()}"))}" : "")}")));

            var midranges = bag.Discs.Where(d => d.Speed >= 4 && d.Speed < 6).ToList();
            if (midranges.Any())
                embedBuilder.AddField("Midrange", string.Join("\n", midranges.Select(d => d.Name)));

            var fairways = bag.Discs.Where(d => d.Speed >= 6 && d.Speed < 9).ToList();
            if (fairways.Any())
                embedBuilder.AddField("Fairway", string.Join("\n", fairways.Select(d => d.Name)));

            var control = bag.Discs.Where(d => d.Speed >= 9 && d.Speed < 11).ToList();
            if (control.Any())
                embedBuilder.AddField("Control", string.Join("\n", control.Select(d => d.Name)));

            var distance = bag.Discs.Where(d => d.Speed > 11).ToList();
            if (distance.Any())
                embedBuilder.AddField("Distance", string.Join("\n", distance.Select(d => d.Name)));

            return embedBuilder.Build();
        }
    }
}
