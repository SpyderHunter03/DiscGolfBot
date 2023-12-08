/* This is the cancellation token we'll use to end the bot if needed(used for most async stuff). */
using DiscgolfBot.Data;
using DiscgolfBot.Services;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

CancellationTokenSource _cts;
DiscordClient _discord;

try
{
    Console.WriteLine("[info] Welcome to my bot!");
    _cts = new CancellationTokenSource();

    // Load the config file(we'll create this shortly)
    Console.WriteLine("[info] Loading config file..");
    var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    var basePath = Directory.GetCurrentDirectory();
    var devSettingsPath = Path.Combine(basePath, $"appsettings.{environmentName}.json");

    var builder = new ConfigurationBuilder()
        .SetBasePath(basePath)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile(Path.Combine(basePath, $"appsettings.{environmentName}.json"), optional: true)
        .AddEnvironmentVariables();

    var _config = builder.Build();

    var services = new ServiceCollection()
        .AddScoped<IDiscRepository, DiscRepository>(dr => new DiscRepository(_config.GetConnectionString("Database")!))
        .AddScoped<IUserRepository, UserRepository>(dr => new UserRepository(_config.GetConnectionString("Database")!))
        .AddScoped<IBagRepository, BagRepository>(dr => new BagRepository(_config.GetConnectionString("Database")!))
        .AddScoped<IErrorService, ErrorService>()
        .BuildServiceProvider();

    // Create the DSharpPlus client
    Console.WriteLine("[info] Creating discord client..");
    _discord = new DiscordClient(new DiscordConfiguration
    {
        Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents,
        Token = _config.GetValue<string>("discord:token"),
        TokenType = TokenType.Bot
    });

    // Create the interactivity module(I'll show you how to use this later on)
    //_discord.UseInteractivity(new InteractivityConfiguration()
    //{
    //    PollBehaviour = PollBehaviour.KeepEmojis,
    //    Timeout = TimeSpan.FromSeconds(30)
    //});

    //SetupCommands(_config);
    
    SetupSlashCommands(services);

    RunAsync().Wait();
}
catch (Exception ex)
{
    // This will catch any exceptions that occur during the operation/setup of your bot.
    Console.Error.WriteLine(ex.ToString());
}

async Task RunAsync()
{
    // Connect to discord's service
    Console.WriteLine("Connecting..");
    await _discord.ConnectAsync();
    Console.WriteLine("Connected!");

    // Keep the bot running until the cancellation token requests we stop
    while (!_cts.IsCancellationRequested)
        await Task.Delay(TimeSpan.FromMinutes(1));
}

void SetupCommands(IConfigurationRoot _config)
{
    var prefixConfiguration = _config.GetValue<string>("discord:commandPrefix") ??
        throw new InvalidDataException("No Discord:commandPrefix value");

    var prefixes = prefixConfiguration.ToCharArray().Select(c => $"{c}");
    Console.WriteLine($"[info] Command prefixes: {string.Join(',', prefixes)}");

    // Build dependancies and then create the commands module.
    var commands = _discord.UseCommandsNext(new CommandsNextConfiguration
    {
        StringPrefixes = prefixes, // Load the command prefix(what comes before the command, eg "!" or "/") from our config file
    });

    // Add command loading
    Console.WriteLine("[info] Loading command modules..");

    commands.RegisterCommands(Assembly.GetExecutingAssembly());

    Console.WriteLine($"[info] {commands.RegisteredCommands.Count} command modules loaded");
}

void SetupSlashCommands(ServiceProvider services)
{
    var slashCommands = _discord.UseSlashCommands(new SlashCommandsConfiguration
    {
        Services = services
    });

    slashCommands.SlashCommandErrored += SlashCommandErrored;
    slashCommands.AutocompleteErrored += AutocompleteErrored;
    Console.WriteLine("[info] Loading slash command modules..");
    //await _discord.BulkOverwriteGlobalApplicationCommandsAsync(Array.Empty<DiscordApplicationCommand>());
    //await _discord.BulkOverwriteGuildApplicationCommandsAsync(1037730809244823592, Array.Empty<DiscordApplicationCommand>());


    var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    ulong? slashCommandsGuildId = (environmentName?.ToLower().Equals("Development".ToLower()) ?? false) ?
        1037730809244823592 : null;

    var assembly = Assembly.GetExecutingAssembly();
    slashCommands.RegisterCommands(assembly, slashCommandsGuildId);

    var slashCommandClasses = assembly.GetTypes()
                    .SelectMany(t => t.GetMethods(),
                                (t, m) => new { Type = t, Method = m, Attributes = m.GetCustomAttributes(typeof(SlashCommandAttribute), true) })
                    .Where(x => x.Attributes.Any())
                    .ToList();

    Console.WriteLine($"[info] {slashCommandClasses.Count} slash command modules loaded:");
    foreach(var slashCommand in slashCommandClasses)
        Console.WriteLine($"\t[info] {slashCommand.Type.Name} module loaded for {(slashCommandsGuildId.HasValue ? $"guild {slashCommandsGuildId.Value}" : "all guilds")}");
}

async Task AutocompleteErrored(SlashCommandsExtension s, AutocompleteErrorEventArgs e)
{
    Console.WriteLine($"Autocomplete errored: {e.Exception.Message}");
}

async Task SlashCommandErrored(SlashCommandsExtension s, SlashCommandErrorEventArgs e)
{
    Console.WriteLine($"Slash command errored: {e.Exception.Message}");
}
