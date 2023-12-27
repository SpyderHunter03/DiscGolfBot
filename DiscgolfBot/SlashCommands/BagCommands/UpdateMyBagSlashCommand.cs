using DiscgolfBot.Data;
using DiscgolfBot.Data.Models;
using DiscgolfBot.Data.Models.ViewModels;
using DiscgolfBot.Services;
using DiscgolfBot.SlashCommands.ChoiceProviders;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscgolfBot.SlashCommands.BagCommands
{
    public class UpdateMyBagSlashCommand : ApplicationCommandModule
    {
        public IDiscRepository _discRespository { private get; set; } // The get accessor is optionally public, but the set accessor must be public.
        public IBagRepository _bagRespository { private get; set; } // The get accessor is optionally public, but the set accessor must be public.
        public IErrorService _errorService { private get; set; } // The get accessor is optionally public, but the set accessor must be public.

        [SlashCommand("updatemybag", "Update your bag with specific discs for your molds")]
        public async Task Command(InteractionContext ctx,
            [Autocomplete(typeof(DiscChoiceProvider))][Option("name", "Disc Name")] string discName,
            [Autocomplete(typeof(PlasticChoiceProvider))][Option("plastic", "Disc Plastic")] string plasticName,
            [Option("weight", "Disc Weight")] double? weight = null,
            [Option("description", "Disc Description")] string? description = null,
            [Option("speed", "Disc Speed")] double? speed = null,
            [Option("glide", "Disc Glide")] double? glide = null,
            [Option("turn", "Disc Turn")] double? turn = null,
            [Option("fade", "Disc Fade")] double? fade = null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent($"{ctx.Member.DisplayName} called /updatemybag {discName} {plasticName}{(weight.HasValue ? $" {weight.Value}" : "")}{(!string.IsNullOrWhiteSpace(description) ? $" {description}" : "")}{(speed.HasValue ? $" {speed.Value}" : "")}{(glide.HasValue ? $" {glide.Value}" : "")}{(turn.HasValue ? $" {turn.Value}" : "")}{(fade.HasValue ? $" {fade.Value}" : "")}")
            );

            try
            {
                var userName = ctx.Member.DisplayName;
                var userId = ctx.Member.Id;

                var disc = await _discRespository.GetDisc(discName);
                if (disc == null)
                {
                    await ctx.Channel.SendMessageAsync(GetDiscDoesNotExistEmbed(discName));
                    return;
                }

                var baggedDiscs = await _bagRespository.GetBaggedDiscs(userId);
                if (baggedDiscs == null || !baggedDiscs.Discs.Any())
                {
                    //No discs in the bag
                    await ctx.Channel.SendMessageAsync(NoDiscsInBagEmbed(userName, baggedDiscs));
                    return;
                }

                //Add mold to bag if not already contained
                if (!baggedDiscs.Discs.Any(d => d.Name.Equals(disc.Name)))
                    await _bagRespository.AddDiscToBag(disc.Id, baggedDiscs.Id);

                var plastic = !string.IsNullOrWhiteSpace(plasticName) ? await _discRespository.GetPlastic(plasticName, disc.ManufacturerId) : null;
                if (!string.IsNullOrWhiteSpace(plasticName) && plastic == null)
                {
                    await ctx.Channel.SendMessageAsync(PlasticNotFoundEmbed(plasticName));
                    return;
                }

                var myBagDisc = await _bagRespository.AddMyBagDisc(baggedDiscs.Id, disc.Id, plastic?.Id, weight, description, speed ?? (double)disc.Speed, glide ?? (double)disc.Glide, turn ?? (double)disc.Turn, fade ?? (double)disc.Fade);

                await ctx.Channel.SendMessageAsync(AddedDiscToMyBagEmbed(myBagDisc, baggedDiscs, disc, plastic, weight, description));
                return;
            }
            catch (Exception ex)
            {
                await _errorService.CommandErrorThrown(ex, ctx, $"{ctx.Member.DisplayName} called /updatemybag {discName} {plasticName}{(weight.HasValue ? $" {weight.Value}" : "")}{(!string.IsNullOrWhiteSpace(description) ? $" {description}" : "")}{(speed.HasValue ? $" {speed.Value}" : "")}{(glide.HasValue ? $" {glide.Value}" : "")}{(turn.HasValue ? $" {turn.Value}" : "")}{(fade.HasValue ? $" {fade.Value}" : "")}");
            }
        }

        protected static DiscordEmbed GetDiscDoesNotExistEmbed(string discName) =>
            new DiscordEmbedBuilder()
                    .WithTitle($"Disc Unavailable")
                    .WithDescription($"{discName} doesn't exist in the database")
                    .WithColor(DiscordColor.Red)
                    .Build();

        protected static DiscordEmbed NoDiscsInBagEmbed(string userName, BaggedDiscs? baggedDiscs) =>
            new DiscordEmbedBuilder()
                    .WithTitle($"{baggedDiscs?.BagName ?? $"{userName}'s bag"}")
                    .WithDescription($"{userName} doesn't have any discs in this bag")
                    .WithColor(DiscordColor.Orange)
                    .Build();

        protected static DiscordEmbed PlasticNotFoundEmbed(string plasticName) =>
            new DiscordEmbedBuilder()
                    .WithTitle($"Plastic Unavailable")
                    .WithDescription($"{plasticName} doesn't exist in the database")
                    .WithColor(DiscordColor.Red)
                    .Build();

        protected static DiscordEmbed AddedDiscToMyBagEmbed(MyBag myBag, BaggedDiscs bag, DiscDetails disc, DiscPlasticDetails? plastic = null, double? weight = null, string? description = null)
        {
            var embedBuilder = new DiscordEmbedBuilder()
                    .WithTitle($"Disc Added To {bag.BagName ?? "Main Bag"}")
                    .WithDescription($"{disc.Name} added to bag.")
                    .WithColor(DiscordColor.Azure);

            embedBuilder.AddField("Flight Numbers", $"({myBag.Speed}, {myBag.Glide}, {myBag.Turn}, {myBag.Fade})");

            if (plastic != null)
                embedBuilder.AddField("Plastic", plastic.Name);

            if (!weight.HasValue)
                embedBuilder.AddField("Weight", $"{weight}g");

            if (!string.IsNullOrWhiteSpace(description))
                embedBuilder.AddField("Description", description);

            return embedBuilder.Build();
        }
            
    }
}
