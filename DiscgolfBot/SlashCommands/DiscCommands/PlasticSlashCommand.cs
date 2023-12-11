using DiscgolfBot.Attributes;
using DiscgolfBot.Data;
using DiscgolfBot.Data.Models;
using DiscgolfBot.Data.Models.ViewModels;
using DiscgolfBot.Services;
using DiscgolfBot.SlashCommands.ChoiceProviders;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscgolfBot.SlashCommands.DiscCommands
{
    public class PlasticSlashCommand : ApplicationCommandModule
    {
        public IDiscRepository _discRespository { private get; set; } // The get accessor is optionally public, but the set accessor must be public.
        public IErrorService _errorService { private get; set; } // The get accessor is optionally public, but the set accessor must be public.

        [SlashCommand("plastic", "Get plastic information")]
        public async Task Command(InteractionContext ctx,
            [Autocomplete(typeof(ManufacturerChoiceProvider))][Option("manufacturer", "Manufacturer", true)] string manufacturer)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent($"{ctx.Member.DisplayName} called /plastic {manufacturer}")
            );

            try
            {
                var plastics = await _discRespository.GetPlastics(manufacturer);
                if (plastics == null || !plastics.Any())
                {
                    await ctx.Channel.SendMessageAsync(NoPlasticsForManufacturerEmbed(manufacturer));
                    return;
                }

                await ctx.Channel.SendMessageAsync(GetPlasticEmbed(plastics.OrderBy(p => p.Name)));
            }
            catch (Exception ex)
            {
                await _errorService.CommandErrorThrown(ex, ctx, $"{ctx.Member.DisplayName} called /plastic {manufacturer}");
            }
        }

        protected static DiscordEmbed GetPlasticEmbed(IEnumerable<DiscPlasticDetails> discPlastics) =>
            new DiscordEmbedBuilder()
                    .WithTitle($"All Plastics Made By {discPlastics.First().ManufacturerName}")
                    .AddField($"Plastics", string.Join("\n", discPlastics.Select(p => p.Name)))
                    .WithColor(DiscordColor.Azure)
                    .Build();

        protected static DiscordEmbed NoPlasticsForManufacturerEmbed(string manufacturerName) =>
            new DiscordEmbedBuilder()
                    .WithTitle($"{manufacturerName} plastics not found")
                    .WithDescription($"{manufacturerName} doesn't have any plastics in our database.")
                    .WithColor(DiscordColor.Red)
                    .Build();
    }
}
