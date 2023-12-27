using DiscgolfBot.Data;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;

namespace DiscgolfBot.SlashCommands.ChoiceProviders
{
    public class PlasticChoiceProvider : IAutocompleteProvider
    {
        public async Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
        {
            var discRepository = ctx.Services.GetService<IDiscRepository>();
            var plastics = await discRepository.GetPlastics();
            if (string.IsNullOrWhiteSpace(ctx.OptionValue as string))
                return plastics.OrderBy(m => m.Name).DistinctBy(m => m.Name).Select(m => new DiscordAutoCompleteChoice(m.Name, m.Name)).Take(25);

            var optionValue = ctx.OptionValue as string;
            return plastics
                .Where(m => m.Name.Contains(optionValue, StringComparison.InvariantCultureIgnoreCase))
                .OrderBy(m => m.Name)
                .DistinctBy(m => m.Name)
                .Select(m => new DiscordAutoCompleteChoice(m.Name, m.Name))
                .Take(25);
        }
    }
}
