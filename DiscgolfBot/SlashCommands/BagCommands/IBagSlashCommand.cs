using DiscgolfBot.Attributes;
using DiscgolfBot.Data;
using DiscgolfBot.Data.Models;
using DiscgolfBot.Services;
using DiscgolfBot.SlashCommands.ChoiceProviders;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscgolfBot.SlashCommands.BagCommands
{
    public class IBagSlashCommand : ApplicationCommandModule
    {
        public IDiscRepository _discRespository { private get; set; } // The get accessor is optionally public, but the set accessor must be public.
        public IBagRepository _bagRespository { private get; set; } // The get accessor is optionally public, but the set accessor must be public.
        public IErrorService _errorService { private get; set; } // The get accessor is optionally public, but the set accessor must be public.

        [SlashCommand("ibag", "Add/Remove disc to/from bag")]
        public async Task Command(InteractionContext ctx,
            [Autocomplete(typeof(DiscChoiceProvider))] [Option("name", "Disc Name")] string discName)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent($"{ctx.Member.DisplayName} called /ibag {discName}")
            );

            try
            {
                var userId = ctx.Member.Id;

                var disc = await _discRespository.GetDisc(discName);
                if (disc == null)
                {
                    await ctx.Channel.SendMessageAsync(GetDiscDoesNotExistEmbed(discName));
                    return;
                }

                var bags = await _bagRespository.GetBags(userId);
                var bag = bags?.FirstOrDefault(b => b.MultiBagNumber == 0);
                if (bag == null)
                {
                    bag = await _bagRespository.CreateBag(userId);
                    var insertedDiscIntoBag = await _bagRespository.AddDiscToBag(disc.Id, bag.Id);
                    await ctx.Channel.SendMessageAsync(GetAddedToBagEmbed(disc));
                    return;
                }

                var baggedDiscs = await _bagRespository.GetBaggedDiscs(bag.Id);
                var baggedDisc = baggedDiscs?.Discs?.FirstOrDefault(bd => bd.Id == disc.Id);
                if (baggedDisc == null)
                {
                    var insertedDiscIntoBag = await _bagRespository.AddDiscToBag(disc.Id, bag.Id);
                    await ctx.Channel.SendMessageAsync(GetAddedToBagEmbed(disc));
                    return;
                }

                var isDiscRemovedFromBag = await _bagRespository.RemoveDiscFromBag(baggedDisc.Id, baggedDiscs.Id);
                await ctx.Channel.SendMessageAsync(GetRemovedFromBagEmbed(disc));
                return;
            }
            catch (Exception ex)
            {
                await _errorService.CommandErrorThrown(ex, ctx, $"{ctx.Member.DisplayName} called /ibag {discName}");
            }
        }

        protected static DiscordEmbed GetAddedToBagEmbed(Disc disc) =>
            new DiscordEmbedBuilder()
                    .WithTitle("Disc Added To Bag")
                    .WithDescription($"The disc, {disc.Name}, has been added to your bag.")
                    .WithColor(DiscordColor.Azure)
                    .Build();

        protected static DiscordEmbed GetRemovedFromBagEmbed(Disc disc) =>
            new DiscordEmbedBuilder()
                    .WithTitle("Disc Removed From Bag")
                    .WithDescription($"The disc, {disc.Name}, has been removed from your bag.")
                    .WithColor(DiscordColor.Azure)
                    .Build();

        protected static DiscordEmbed GetDiscDoesNotExistEmbed(string discName) =>
            new DiscordEmbedBuilder()
                    .WithTitle($"Disc Unavailable")
                    .WithDescription($"{discName} doesn't exist in the database")
                    .WithColor(DiscordColor.Red)
                    .Build();
    }
}
