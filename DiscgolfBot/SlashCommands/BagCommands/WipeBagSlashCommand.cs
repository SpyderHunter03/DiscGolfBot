using DiscgolfBot.Data;
using DiscgolfBot.Data.Models;
using DiscgolfBot.Data.Models.ViewModels;
using DiscgolfBot.Services;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscgolfBot.SlashCommands.BagCommands
{
    public class WipeBagSlashCommand : ApplicationCommandModule
    {
        public IBagRepository _bagRespository { private get; set; } // The get accessor is optionally public, but the set accessor must be public.
        public IErrorService _errorService { private get; set; } // The get accessor is optionally public, but the set accessor must be public.

        [SlashCommand("wipebag", "Clear the contents of a bag")]
        public async Task Command(InteractionContext ctx)
        {
            var userName = ctx.Member.DisplayName;

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent($"{userName} called /wipebag")
            );

            try
            {
                //check to make sure the user is real
                var userId = ctx.Member.Id;
                var bags = await _bagRespository.GetBags(userId);
                if (bags == null || !bags.Any())
                {
                    //No bags setup
                    await ctx.Channel.SendMessageAsync(NoBagsEmbed(userName));
                    return;
                }

                var bag = bags.FirstOrDefault(b => b.MultiBagNumber == 0) ?? bags.First(); //Only working with 1 bag for now
                
                var removedDiscs = await _bagRespository.RemoveAllDiscsFromBag(bag.Id);

                await ctx.Channel.SendMessageAsync(RemovedDiscsEmbed(bag));
                return;
            }
            catch (Exception ex)
            {
                await _errorService.CommandErrorThrown(ex, ctx, $"{userName} called /wipebag");
            }
        }

        protected static DiscordEmbed RemovedDiscsEmbed(Bag bag)
        {
            var embedBuilder = new DiscordEmbedBuilder()
                    .WithTitle($"{bag.BagName ?? "Main Bag"} wiped")
                    .WithDescription($"Your bag has been wiped!")
                    .WithColor(DiscordColor.Azure);

            return embedBuilder.Build();
        }


        protected static DiscordEmbed NoBagsEmbed(string userName) =>
            new DiscordEmbedBuilder()
                    .WithTitle($"No bags found")
                    .WithDescription($"{userName} doesn't have any bags")
                    .WithColor(DiscordColor.Yellow)
                    .Build();
    }
}
