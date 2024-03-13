

using AusDevsBot;
using Discord;
using Discord.WebSocket;

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
    var s = x.Split('=', 1);
    return (s[0], s[1]);
});



var discordToken = tokens.First(x => x.Item1 == "TOKEN").Item2;
ulong guildId = ulong.Parse(tokens.First(x => x.Item1 == "GUILD_ID").Item2);;



DiscordSocketClient client = new DiscordSocketClient();

var builder = new BotCommandBuilder();

// todo use serilog
client.Log += message =>
{
    Console.WriteLine($"[{message.Severity}] {message.Message}");
    return Task.CompletedTask;
};

client.Ready += async () =>
{
    var guild = client.GetGuild(guildId);
    builder = await builder
        .WithGuild(guild)
        .AddCommand("test-command", "A test command", async command =>
        {
            await command.RespondAsync("Command builder responding");
        });

    client.SlashCommandExecuted += builder.HandleIncomingSlashCommands;
};


await client.LoginAsync(TokenType.Bot, discordToken);
await client.StartAsync();



// infinite delay
await Task.Delay(-1);
