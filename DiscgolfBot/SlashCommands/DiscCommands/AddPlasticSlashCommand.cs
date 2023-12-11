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
    public class AddPlasticSlashCommand : ApplicationCommandModule
    {
        public IDiscRepository _discRespository { private get; set; } // The get accessor is optionally public, but the set accessor must be public.
        public IErrorService _errorService { private get; set; } // The get accessor is optionally public, but the set accessor must be public.

        [SlashCommand("addplastic", "Add plastic information")]
        [RequireAdmin]
        public async Task Command(InteractionContext ctx,
            [Autocomplete(typeof(ManufacturerChoiceProvider))][Option("manufacturer", "Manufacturer", true)] string manufacturer,
            [Option("name", "Plastic Name")] string plasticName)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent($"{ctx.Member.DisplayName} called /addplastic {manufacturer} {plasticName}")
            );

            try
            {
                var plastic = await _discRespository.GetPlastic(plasticName, manufacturer);
                if (plastic != null)
                {
                    await ctx.Channel.SendMessageAsync(GetPlasticAlreadyExistsEmbed(plastic));
                    return;
                }

                var dbManufacturer = await _discRespository.GetManufacturer(manufacturer) ?? await _discRespository.AddManufacturer(manufacturer);

                var insertedPlastic = await _discRespository.AddPlastic(dbManufacturer.Id, plasticName);
                var embed = insertedPlastic != null ? GetPlasticEmbed(insertedPlastic) : GetFailedQueryEmbed(plasticName);

                await ctx.Channel.SendMessageAsync(embed);
            }
            catch (Exception ex)
            {
                await _errorService.CommandErrorThrown(ex, ctx, $"{ctx.Member.DisplayName} called /addplastic {manufacturer} {plasticName}");
            }
        }

        protected static DiscordEmbed GetPlasticEmbed(DiscPlasticDetails discPlastic) =>
            new DiscordEmbedBuilder()
                    .WithTitle("Plastic Added")
                    .WithDescription($"The plastic, {discPlastic.Name}, has been added to the database.")
                    .WithColor(DiscordColor.Azure)
                    .Build();

        protected static DiscordEmbed GetPlasticAlreadyExistsEmbed(DiscPlasticDetails plastic) =>
            new DiscordEmbedBuilder()
                    .WithTitle($"{plastic.Name} not added")
                    .WithDescription($"{plastic.Name} already exists in the database for {plastic.ManufacturerName}")
                    .WithColor(DiscordColor.Red)
                    .Build();

        protected static DiscordEmbed GetFailedQueryEmbed(string requestedPlasticName) =>
            new DiscordEmbedBuilder()
                    .WithTitle("Unable to add plastic")
                    .WithDescription($"We were unable to add the plastic '{requestedPlasticName}'.")
                    .WithColor(DiscordColor.Red)
                    .Build();
    }
}
