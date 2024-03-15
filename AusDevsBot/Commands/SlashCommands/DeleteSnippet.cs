using AusDevsBot.Data;
using Discord.WebSocket;

namespace AusDevsBot.Commands.SlashCommands;

public class DeleteSnippet : ISlashCommand
{
    private BotDbContext _context;
    
    public DeleteSnippet(BotDbContext context)
    {
        _context = context;
    }
    
    public async Task HandleCommand(SocketSlashCommand command)
    {
        var user = await _context.Users.FindAsync(command.User.Id);
        if (user == null || user.NumberOfSnippets == 0)
        {
            await command.RespondAsync("Looks like you haven't saved any snippets");
            return;
        }

        var snippetId = (string?) command.Data.Options.FirstOrDefault(x => x.Name == "id")?.Value;
        if (string.IsNullOrEmpty(snippetId))
        {
            await command.RespondAsync("Snippet id is required");
            return;
        }

        SavedSnippet? snippet;
        if (Guid.TryParse(snippetId, out Guid id))
        {
            // guid supplied
            snippet = user.SavedSnippets.Find(x => x.SnippetId == id);
        }
        else
        {
            // quick save id supplied
            snippet = user.SavedSnippets.Find(x => x.QuickSaveId == snippetId);
        }

        _context.SavedSnippets.Remove(snippet);
        await _context.SaveChangesAsync();
        
        await command.RespondAsync("Deleted snippet");
    }
}