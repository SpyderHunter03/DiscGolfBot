using DiscgolfBot.Attributes;
using DiscgolfBot.Data;
using DiscgolfBot.Data.Models;
using DiscgolfBot.Services;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscgolfBot.SlashCommands.DiscCommands
{
    public class AddDiscSlashCommand : ApplicationCommandModule
    {
        public IDiscRepository _discRespository { private get; set; } // The get accessor is optionally public, but the set accessor must be public.
        public IErrorService _errorService { private get; set; } // The get accessor is optionally public, but the set accessor must be public.

        [SlashCommand("adddisc", "Add disc information")]
        [RequireAdmin]
        public async Task Command(InteractionContext ctx, 
            [Option("name", "Disc Name")] string discName, 
            [Option("manufacturer", "Manufacturer")] string manufacturer,
            [Option("speed", "Speed")] double speed,
            [Option("glide", "Speed")] double glide,
            [Option("turn", "Speed")] double turn,
            [Option("fade", "Speed")] double fade)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent($"{ctx.Member.DisplayName} called /adddisc {discName} {manufacturer} {speed} {glide} {turn} {fade}")
            );

            try
            {
                var disc = await _discRespository.GetDisc(discName);
                if (disc != null)
                {
                    await ctx.Channel.SendMessageAsync(GetDiscAlreadyExistsEmbed(disc));
                    return;
                }

                var insertedDisc = await _discRespository.AddDisc(discName, manufacturer, speed, glide, turn, fade);
                var embed = insertedDisc != null ? GetDiscEmbed(insertedDisc) : GetFailedQueryEmbed(discName);

                await ctx.Channel.SendMessageAsync(embed);
            }
            catch (Exception ex)
            {
                await _errorService.CommandErrorThrown(ex, ctx, $"{ctx.Member.DisplayName} called /adddisc {discName} {manufacturer} {speed} {glide} {turn} {fade}");
            }
        }

        protected static DiscordEmbed GetDiscEmbed(Disc disc) =>
            new DiscordEmbedBuilder()
                    .WithTitle("Disc Added")
                    .WithDescription($"The disc, {disc.Name}, has been added to the database.")
                    .WithColor(DiscordColor.Azure)
                    .Build();

        protected static DiscordEmbed GetDiscAlreadyExistsEmbed(Disc disc) =>
            new DiscordEmbedBuilder()
                    .WithTitle($"{disc.Name} already exists in the database")
                    .WithDescription($"{disc.Manufacturer}\n{disc.Speed}, {disc.Glide}, {disc.Turn}, {disc.Fade}\n[PDGA](https://www.pdga.com/technical-standards/equipment-certification/discs/{disc.Name})")
                    .WithColor(DiscordColor.Red)
                    .Build();

        protected static DiscordEmbed GetFailedQueryEmbed(string requestedDiscName) =>
            new DiscordEmbedBuilder()
                    .WithTitle("Unable to add disc")
                    .WithDescription($"We were unable to add the disc '{requestedDiscName}'.")
                    .WithColor(DiscordColor.Red)
                    .Build();
    }
}
