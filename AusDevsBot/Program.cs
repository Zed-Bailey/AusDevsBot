

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
    var s = x.Split('=');
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
    try
    {
        var guild = client.GetGuild(guildId);
        builder.WithGuild(guild);
        
        await builder.AddCommand("snippet", "Save a code snippet for later", async command =>
            {
                var code = (string) command.Data.Options.First(x => x.Name == "code").Value;
                var language = (string)  command.Data.Options.First(x => x.Name == "language").Value;
                var quickId = (string?) command.Data.Options.FirstOrDefault(x => x.Name == "quick-id")?.Value;

                var embedBuilder = new EmbedBuilder()
                    .WithAuthor(command.User)
                    .WithTitle("Snippet - " + language)
                    .WithDescription(code)
                    .WithColor(Color.Gold)
                    .WithCurrentTimestamp();

                await command.RespondAsync(embed: embedBuilder.Build());

            }, new SlashCommandOptionBuilder()
            {
                Name = "code",
                Type = ApplicationCommandOptionType.String,
                IsRequired = true,
                Description = "Code snippet to save"
            }, new SlashCommandOptionBuilder()
            {
                Name = "language",
                Type = ApplicationCommandOptionType.String,
                IsRequired = true,
                Description = "programming language of snippet"
            }, new SlashCommandOptionBuilder()
            {
                Name = "quick-id",
                Type = ApplicationCommandOptionType.String,
                IsRequired = false,
                Description = "An id to identify this snippet to quickly find it later"
            });

        await builder.AddCommand("fetch", "fetch a snippet by id", async command =>
        {

        }, new SlashCommandOptionBuilder()
        {
            Name = "id",
            Description = "Get a snippet by id",
            Type = ApplicationCommandOptionType.String,
        });
            
        client.SlashCommandExecuted += builder.HandleIncomingSlashCommands;
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
};


await client.LoginAsync(TokenType.Bot, discordToken);
await client.StartAsync();



// infinite delay
await Task.Delay(-1);
