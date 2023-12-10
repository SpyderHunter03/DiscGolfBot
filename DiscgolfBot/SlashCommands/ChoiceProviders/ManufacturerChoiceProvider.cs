using DiscgolfBot.Data;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;

namespace DiscgolfBot.SlashCommands.ChoiceProviders
{
    public class ManufacturerChoiceProvider : IAutocompleteProvider
    {
        public async Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
        {
            var discRepository = ctx.Services.GetService<IDiscRepository>();
            var manufacturers = await discRepository.GetManufacturers();
            if (string.IsNullOrWhiteSpace(ctx.OptionValue as string))
                return manufacturers.OrderBy(m => m.Name).Select(m => new DiscordAutoCompleteChoice(m.Name, m.Name)).Take(25);

            var optionValue = ctx.OptionValue as string;
            return manufacturers
                .Where(m => m.Name.Contains(optionValue, StringComparison.InvariantCultureIgnoreCase))
                .OrderBy(m => m.Name)
                .Select(m => new DiscordAutoCompleteChoice(m.Name, m.Name))
                .Take(25);
        }
    }
}
