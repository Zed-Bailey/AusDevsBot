using AusDevsBot.Data;
using Discord.WebSocket;

namespace AusDevsBot.Commands.SlashCommands;

public class SnippetStats : ISlashCommand
{
    private BotDbContext _context;
    
    public SnippetStats(BotDbContext context)
    {
        _context = context;
    }
    

    public async Task HandleCommand(SocketSlashCommand command)
    {
        var userId = command.User.Id;
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            await command.RespondAsync("Looks like you haven't saved any snippets yet");
            return;
        }

        var avgLength = user.SavedSnippets?.Average(x => x.Content.Length);
        var msg = $"""
                  saved {user.NumberOfSnippets} snippets
                  average snippet length is {avgLength ?? 0} characters
                  """;
        
        // todo: update to embed
        await command.RespondAsync(msg);
    }
}