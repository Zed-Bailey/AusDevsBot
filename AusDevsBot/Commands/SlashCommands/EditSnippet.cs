using AusDevsBot.Data;
using Discord.WebSocket;

namespace AusDevsBot.Commands.SlashCommands;

public class EditSnippet
{

    public class UpdateSnippetContent : ISlashCommand
    {
        private BotDbContext _context;
        
        public UpdateSnippetContent(BotDbContext context)
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

            if (snippet == null)
            {
                await command.RespondAsync("No snippet was found with a matching guid or quick-save id");
                return;
            }

            var newContent = (string?) command.Data.Options.FirstOrDefault(x => x.Name == "content")?.Value;
            snippet.Content = newContent;
            await _context.SaveChangesAsync();

            await command.RespondAsync($"Update snippet with id: '{snippetId}' with the new content");

        }
    }
    
    
    public class RenameSnippet : ISlashCommand
    {
        private BotDbContext _context;
        
        public RenameSnippet(BotDbContext context)
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

            if (snippet == null)
            {
                await command.RespondAsync("No snippet was found with a matching guid or quick-save id");
                return;
            }
            
            var newId = (string?) command.Data.Options.FirstOrDefault(x => x.Name == "new-id")?.Value;
            snippet.QuickSaveId = newId;
            await _context.SaveChangesAsync();

            await command.RespondAsync($"updated snippet id from '{snippetId}' to '{newId}'");
        }
    }
}