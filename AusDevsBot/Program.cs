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

// socket config has to be added before the client
builder.Services.AddSingleton(socketConfig);
builder.Services.AddSingleton<DiscordSocketClient>();

var host = builder.Build();

var client = host.Services.GetRequiredService<DiscordSocketClient>();

// load the text-based commands from the assembly
CommandService commands = new CommandService(new CommandServiceConfig()
{
    CaseSensitiveCommands = false,
});
// add all the text based commands
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
        
        // register all the slash commands
        
        await botCommandBuilder.AddCommand<SaveSnippet>("snippet", "Save a snippet for later", new SlashCommandOptionBuilder()
            {
                Name = "content",
                Type = ApplicationCommandOptionType.String,
                IsRequired = true,
                Description = "Snippet to save"
            }, new SlashCommandOptionBuilder()
            {
                Name = "quick-id",
                Type = ApplicationCommandOptionType.String,
                IsRequired = false,
                Description = "An id to identify this snippet to quickly find it later"
            });
        
        
        await botCommandBuilder.AddCommand<FetchSnippets.DetailSnippet>("detail", "View an entire snippet",
            new SlashCommandOptionBuilder()
            {
                Name = "id",
                Description = "Id of the snippet to view, either guid or quick-save id",
                IsRequired = true,
                Type = ApplicationCommandOptionType.String
            });
        
        await botCommandBuilder.AddCommand<FetchSnippets.MySnippets>("my-snippets", "View your snippets", new SlashCommandOptionBuilder()
        {
            Name = "page",
            IsRequired = false,
            Description = "The page of snippets to get",
            Type = ApplicationCommandOptionType.Integer,
            MinValue = 0
        });

        
        await botCommandBuilder.AddCommand<SnippetStats>("snippet-stats", "See the snippet stats");
        
        
        await botCommandBuilder.AddCommand<DeleteSnippet>("delete", "Delete a snippet",
            new SlashCommandOptionBuilder()
            {
                Name = "id",
                Description = "Id of the snippet to delete, either guid or quick-save id",
                IsRequired = true,
                Type = ApplicationCommandOptionType.String
            });
        
        await botCommandBuilder.AddCommand<EditSnippet.RenameSnippet>("rename", "Rename a snippets id",
            new SlashCommandOptionBuilder()
            {
                Name = "id",
                Description = "Current id of the snippet to update, either guid or quick-save id",
                IsRequired = true,
                Type = ApplicationCommandOptionType.String
            },
            new SlashCommandOptionBuilder()
            {
                Name = "new-id",
                Description = "The new id of the snippet",
                IsRequired = true,
                Type = ApplicationCommandOptionType.String
            });
        
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