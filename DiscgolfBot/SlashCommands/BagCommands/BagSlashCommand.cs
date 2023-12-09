using DiscgolfBot.Attributes;
using DiscgolfBot.Data;
using DiscgolfBot.Data.Models;
using DiscgolfBot.Services;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscgolfBot.SlashCommands.BagCommands
{
    public class BagSlashCommand : ApplicationCommandModule
    {
        public IDiscRepository _discRespository { private get; set; } // The get accessor is optionally public, but the set accessor must be public.
        public IBagRepository _bagRespository { private get; set; } // The get accessor is optionally public, but the set accessor must be public.
        public IErrorService _errorService { private get; set; } // The get accessor is optionally public, but the set accessor must be public.

        [SlashCommand("bag", "Look at the contents of a bag")]
        [RequireAdmin]
        public async Task Command(InteractionContext ctx,
            [Option("user", "Bag Owner")] DiscordUser? user = null)
        {
            var userName = (user as DiscordMember)?.DisplayName ?? ctx.Member.DisplayName;

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent($"{ctx.Member.DisplayName} called /bag {(user as DiscordMember)?.DisplayName ?? ""}")
            );

            try
            {
                //check to make sure the user is real
                var userId = user?.Id ?? ctx.Member.Id;

                // get bag with bag options => 
                var baggedDiscs = await _bagRespository.GetBaggedDiscsUpgraded(userId);

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
                await _errorService.CommandErrorThrown(ex, ctx, $"{ctx.Member.DisplayName} called /bag {(user != null ? user.Id : "")}");
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

            if (bag.PutterName != null)
                embedBuilder.AddField("Putting with:", bag.PutterName);

            var putters = bag.Discs.Where(d => d.Speed < 4).ToList();
            if (putters.Any())
                embedBuilder.AddField("Putter", string.Join(", ", putters.Select(d => d.Name)));

            var midranges = bag.Discs.Where(d => d.Speed >= 4 && d.Speed < 6).ToList();
            if (midranges.Any())
                embedBuilder.AddField("Midrange", string.Join(", ", midranges.Select(d => d.Name)));

            var fairways = bag.Discs.Where(d => d.Speed >= 6 && d.Speed < 9).ToList();
            if (fairways.Any())
                embedBuilder.AddField("Fairway", string.Join(", ", fairways.Select(d => d.Name)));

            var control = bag.Discs.Where(d => d.Speed >= 9 && d.Speed < 11).ToList();
            if (control.Any())
                embedBuilder.AddField("Control", string.Join(", ", control.Select(d => d.Name)));

            var distance = bag.Discs.Where(d => d.Speed > 11).ToList();
            if (distance.Any())
                embedBuilder.AddField("Distance", string.Join(", ", distance.Select(d => d.Name)));

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
