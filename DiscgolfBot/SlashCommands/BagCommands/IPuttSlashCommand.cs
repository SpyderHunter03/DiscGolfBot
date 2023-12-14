using DiscgolfBot.Data;
using DiscgolfBot.Data.Models;
using DiscgolfBot.Services;
using DiscgolfBot.SlashCommands.ChoiceProviders;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscgolfBot.SlashCommands.BagCommands
{
    public class IPuttSlashCommand : ApplicationCommandModule
    {
        public IDiscRepository _discRespository { private get; set; } // The get accessor is optionally public, but the set accessor must be public.
        public IBagRepository _bagRespository { private get; set; } // The get accessor is optionally public, but the set accessor must be public.
        public IErrorService _errorService { private get; set; } // The get accessor is optionally public, but the set accessor must be public.

        [SlashCommand("iputt", "Add/Remove disc to/from bag")]
        public async Task Command(InteractionContext ctx,
            [Autocomplete(typeof(DiscChoiceProvider))][Option("name", "Disc Name")] string discName)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent($"{ctx.Member.DisplayName} called /iputt {discName}")
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
                var bag = bags?.FirstOrDefault(b => b.MultiBagNumber == 0) ??
                    await _bagRespository.CreateBag(userId);

                var updatedBag = await _bagRespository.UpdatePutter(bag.Id, disc.Id);
                await ctx.Channel.SendMessageAsync(GetAddedToBagEmbed(disc));
                return;
            }
            catch (Exception ex)
            {
                await _errorService.CommandErrorThrown(ex, ctx, $"{ctx.Member.DisplayName} called /iputt {discName}");
            }
        }

        protected static DiscordEmbed GetAddedToBagEmbed(Disc disc) =>
            new DiscordEmbedBuilder()
                    .WithTitle("Your putter added")
                    .WithDescription($"Your putter, {disc.Name}, has been updated in your bag.")
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
