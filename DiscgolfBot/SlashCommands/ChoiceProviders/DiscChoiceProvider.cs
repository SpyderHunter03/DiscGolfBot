using DiscgolfBot.Data;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;

namespace DiscgolfBot.SlashCommands.ChoiceProviders
{
    public class DiscChoiceProvider : IAutocompleteProvider
    {
        public async Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
        {
            var discRepository = ctx.Services.GetService<IDiscRepository>();
            var discs = await discRepository.GetDiscs();
            if (string.IsNullOrWhiteSpace(ctx.OptionValue as string))
                return discs.OrderBy(d => d.Name).Select(m => new DiscordAutoCompleteChoice(m.Name, m.Name)).Take(25);

            var optionValue = ctx.OptionValue as string;
            return discs
                .Where(m => m.Name.Contains(optionValue, StringComparison.InvariantCultureIgnoreCase))
                .OrderBy(d => d.Name)
                .Select(m => new DiscordAutoCompleteChoice(m.Name, m.Name))
                .Take(25);
        }
    }
}