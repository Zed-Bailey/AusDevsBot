using System.Reflection;
using System.Text;
using AusDevsBot;
using AusDevsBot.Commands;
using AusDevsBot.Commands.SlashCommands;
using AusDevsBot.Data;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// todo fix this to use microsoft.extensions.configuration
var fileLines = await File.ReadLinesAsync(Path.Join(Directory.GetCurrentDirectory(), ".env"))
    .ToListAsync();

if (!fileLines.Any())
{
    Console.WriteLine($"[Error] .env file was not found! looked at: {Path.Join(Directory.GetCurrentDirectory(), ".env")}");
    return;
}

var tokens = fileLines.Select(x =>
{
    var s = x.Split('=');
    return (s[0], s[1]);
});



var discordToken = tokens.First(x => x.Item1 == "TOKEN").Item2;
ulong guildId = ulong.Parse(tokens.First(x => x.Item1 == "GUILD_ID").Item2);;


var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<BotDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DbContext"));
    options.UseLazyLoadingProxies();
});

var socketConfig = new DiscordSocketConfig()
{
    GatewayIntents = GatewayIntents.MessageContent | GatewayIntents.AllUnprivileged
};

builder.Services.AddSingleton(socketConfig);
builder.Services.AddSingleton<DiscordSocketClient>();

var host = builder.Build();

var client = host.Services.GetRequiredService<DiscordSocketClient>();

// load the text-based commands from the assembly
CommandService commands = new CommandService(new CommandServiceConfig()
{
    CaseSensitiveCommands = false,
});

await commands.AddModulesAsync(
    assembly: Assembly.GetEntryAssembly(), 
    services: host.Services
);

Console.WriteLine("registered text commands: " + string.Join(',', commands.Commands.Select(x => x.Name)));
client.MessageReceived += HandleCommandAsync;

// todo use serilog
client.Log += message =>
{
    Console.WriteLine($"[{message.Severity}] {message.Message}");
    return Task.CompletedTask;
};



var botCommandBuilder = new BotCommandBuilder(host.Services);


client.Ready += async () =>
{
    try
    {
        var guild = client.GetGuild(guildId);
        botCommandBuilder.WithGuild(guild);
        
        // await botCommandBuilder.AddCommand("snippet", "Save a code snippet for later", async command =>
        //     {
        //         var code = (string) command.Data.Options.First(x => x.Name == "code").Value;
        //         var language = (string)  command.Data.Options.First(x => x.Name == "language").Value;
        //         var quickId = (string?) command.Data.Options.FirstOrDefault(x => x.Name == "quick-id")?.Value;
        //
        //         var embedBuilder = new EmbedBuilder()
        //             .WithAuthor(command.User)
        //             .WithTitle("Snippet - " + language)
        //             .WithDescription(code)
        //             .WithColor(Color.Gold)
        //             .WithCurrentTimestamp();
        //
        //         await command.RespondAsync(embed: embedBuilder.Build());
        //
        //     }, new SlashCommandOptionBuilder()
        //     {
        //         Name = "code",
        //         Type = ApplicationCommandOptionType.String,
        //         IsRequired = true,
        //         Description = "Code snippet to save"
        //     }, new SlashCommandOptionBuilder()
        //     {
        //         Name = "language",
        //         Type = ApplicationCommandOptionType.String,
        //         IsRequired = true,
        //         Description = "programming language of snippet"
        //     }, new SlashCommandOptionBuilder()
        //     {
        //         Name = "quick-id",
        //         Type = ApplicationCommandOptionType.String,
        //         IsRequired = false,
        //         Description = "An id to identify this snippet to quickly find it later"
        //     });
        //
        // await botCommandBuilder.AddCommand("my-snippets", "View your snippets", async command =>
        // {
        //     var context = host.Services.GetRequiredService<BotDbContext>();
        //     var user = await context.Users.FindAsync(command.User.Id);
        //     if (user == null || user.NumberOfSnippets == 0)
        //     {
        //         await command.RespondAsync("Looks like you haven't saved any snippets");
        //         return;
        //     }
        //     
        //     
        //     var description = user.SavedSnippets.Select(x =>
        //         $"{x.QuickSaveId ?? x.SnippetId.ToString()} - {x.Content.Substring(0, 30)}.....");
        //
        //     var sb = new StringBuilder();
        //     sb.AppendLine(
        //         "+--------------------------------------+-------------------------------------+\n| id                                   | content                             |\n+--------------------------------------+-------------------------------------+");
        //
        //     foreach (var snip in user.SavedSnippets)
        //     {
        //         // 36 is length of guid
        //         sb.AppendLine($"| {snip.QuickSaveId?.PadRight(36) ?? snip.SnippetId.ToString()} | {snip.Content.Substring(0,30).PadRight(30)}..... |");
        //         sb.AppendLine("+--------------------------------------+-------------------------------------+");
        //     }
        //
        //     await command.RespondAsync($"```\n{sb}\n```");
        //
        // });

        await botCommandBuilder.AddCommand<SnippetStats>("snippet-stats", "See the snippet stats");
            
        client.SlashCommandExecuted += botCommandBuilder.HandleIncomingSlashCommands;
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
};


await client.LoginAsync(TokenType.Bot, discordToken);
await client.StartAsync();







host.Run();

return;


async Task HandleCommandAsync(SocketMessage messageParam)
{
    Console.WriteLine("received message");
    // Don't process the command if it was a system message
    var message = messageParam as SocketUserMessage;
    if (message == null) return;
    
    // Create a number to track where the prefix ends and the command begins
    
    int argPos = 0;
    // Determine if the message is a command based on the prefix and make sure no bots trigger commands
    if (!message.HasCharPrefix('!', ref argPos) || message.Author.IsBot) 
        return;
    
    // Create a WebSocket-based command context based on the message
    var context = new SocketCommandContext(client, message);

    // Execute the command with the command context we just
    // created, along with the service provider for precondition checks.
    await commands.ExecuteAsync(
        context: context, 
        argPos: argPos,
        services: host.Services
    );
}