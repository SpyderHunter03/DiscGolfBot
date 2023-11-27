using DiscgolfBot.Services;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Reflection;

namespace DiscgolfBot.SlashCommands
{
    public class HelpSlashCommand : ApplicationCommandModule
    {
        public IErrorService _errorService { private get; set; } // The get accessor is optionally public, but the set accessor must be public.

        [SlashCommand("help", "Get help with bot commands")]
        public async Task Command(InteractionContext ctx, [Option("categoryName", "Category Name")] string? categoryName = null)
        {
           var _responseContent = $"{ctx.Member.DisplayName} called /help {categoryName}";

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent(_responseContent)
            );

            try
            {
                var slashCommandClasses = Assembly.GetExecutingAssembly().GetTypes()
                    .SelectMany(t => t.GetMethods(),
                                (t, m) => new { Type = t, Method = m, Attributes = m.GetCustomAttributes(typeof(SlashCommandAttribute), true) })
                    .Where(x => x.Attributes.Any() && (x.Type.Namespace?.Split('.').Length ?? 0) >= 3)
                    .Select((x) => {
                        var fullCategory = x.Type.Namespace!.Split('.').Last();
                        return new HelpClass
                        {
                            Category = fullCategory![..(fullCategory!.IndexOf("Commands"))],
                            SlashCommandInfo = x.Attributes.Cast<SlashCommandAttribute>().Select(a => new SlashCommand { Name = a.Name, Description = a.Description }).FirstOrDefault()
                        };
                    })
                    .GroupBy(x => x.Category)
                    .ToList();

                var embed = string.IsNullOrWhiteSpace(categoryName) ? 
                                GetHelpEmbed(slashCommandClasses!) :
                            slashCommandClasses.Any(s => s.Key.ToLower().Equals(categoryName.ToLower())) ?
                                GetHelpEmbedForCategory(slashCommandClasses.FirstOrDefault(s => s.Key.ToLower().Equals(categoryName.ToLower()))!) :
                                GetFailedCategoryEmbed(categoryName);


                await ctx.Channel.SendMessageAsync(embed);
            }
            catch (Exception ex)
            {
                await _errorService.CommandErrorThrown(ex, ctx, _responseContent);
            }
        }

        protected static DiscordEmbed GetHelpEmbed(IEnumerable<IGrouping<string, HelpClass>> helpClasses)
        {
            var embed = new DiscordEmbedBuilder()
                    .WithTitle("Discbot Help")
                    .WithDescription("As the bot commands continue to grow, this help file has been broken down into categories.  Use `/help categoryName` to get all the specific commands for that topic.  The first command you will need to get started is `/ibag`!")
                    .WithColor(DiscordColor.Azure);

            foreach(var group in helpClasses)
                embed.AddField(group.Key, $"{(CommandGroupDescriptions.TryGetValue(group.Key, out var desc) ? desc : "")}`{string.Join(", ", group.Select(g => $"/{g.SlashCommandInfo!.Name}"))}`");
            
            return embed.Build();
        }

        protected static DiscordEmbed GetHelpEmbedForCategory(IEnumerable<HelpClass> helpClasses)
        {
            var embed = new DiscordEmbedBuilder()
                    .WithTitle($"Discbot {helpClasses.FirstOrDefault()!.Category} Commands")
                    .WithColor(DiscordColor.Azure);

            foreach (var group in helpClasses)
                embed.AddField($"/{group.SlashCommandInfo!.Name}", group.SlashCommandInfo!.Description);

            return embed.Build();
        }

        protected static DiscordEmbed GetFailedCategoryEmbed(string requestedCategory) =>
            new DiscordEmbedBuilder()
                    .WithTitle("Unable to find category")
                    .WithDescription($"We were unable to find a help category with the name of '{requestedCategory}'.")
                    .WithColor(DiscordColor.Red)
                    .Build();

        private readonly static Dictionary<string, string> CommandGroupDescriptions = new()
        {
            { "Disc", "Commands dealing with discs:\n"}
        };

        protected class HelpClass
        {
            public string Category { get; set; } = string.Empty;
            public SlashCommand? SlashCommandInfo { get; set; }
        }

        protected class SlashCommand
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
        }
    }
}
