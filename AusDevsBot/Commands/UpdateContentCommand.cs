using System.ComponentModel;
using AusDevsBot.Data;
using Discord.Commands;

namespace AusDevsBot.Commands;

public class UpdateContentCommand : ModuleBase<SocketCommandContext>
{
    private BotDbContext _context;

    public UpdateContentCommand(BotDbContext context)
    {
        _context = context;
    }
    
    
    [Command("set-content")]
    [Description("Update the snippet contnet with what's in the replied message")]
    public async Task UpdateContent([Remainder] string snippetId)
    {
        var replyMessage = Context.Message.ReferencedMessage;
        if (replyMessage == null)
        {
            await ReplyAsync("Must reply to a message to use this command");
            return;
        }
        
        var user = await _context.Users.FindAsync(Context.User.Id);
        if (user == null || user.NumberOfSnippets == 0)
        {
            await ReplyAsync("Looks like you haven't saved any snippets");
            return;
        }

        
        if (string.IsNullOrEmpty(snippetId))
        {
            await ReplyAsync("Snippet id is required");
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
            await ReplyAsync("No snippet was found with a matching guid or quick-save id");
            return;
        }

        snippet.Content = replyMessage.CleanContent;
        await _context.SaveChangesAsync();

        await ReplyAsync("Updated snippet content");

    }
}