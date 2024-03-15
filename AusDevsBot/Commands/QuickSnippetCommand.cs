using AusDevsBot.Data;
using Discord.Commands;

namespace AusDevsBot.Commands;

public class QuickSnippetCommand : ModuleBase<SocketCommandContext>
{
    private BotDbContext _context;
    public QuickSnippetCommand(BotDbContext context)
    {
        _context = context;
    }
    

    [Command("snippet")]
    [Summary("Quick saves a message")]
    public async Task QuickSaveMessage([Remainder] string? quickSaveId = null)
    {
        
        var replyMsg = Context.Message.ReferencedMessage;
        if (replyMsg == null)
        {
            await ReplyAsync("To quick save you must reply to a message", messageReference: Context.Message.Reference);
            return;
        }

        var snippet = new SavedSnippet()
        {
            Content = replyMsg.CleanContent,
            QuickSaveId = quickSaveId,
            Language = "text",
            LastUpdated = DateTime.UtcNow,
        };

        var user = await _context.Users.FindAsync(Context.User.Id);
        if (user == null)
        {
            // create a new user
            var newUser = new User()
            {
                UserId = Context.User.Id,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            user = newUser;
        }

        snippet.User = user;
        _context.SavedSnippets.Add(snippet);
        
        user.SavedSnippets.Add(snippet);
        await _context.SaveChangesAsync();
        
        await ReplyAsync($"Saved snippet, id: {snippet.QuickSaveId ?? snippet.SnippetId.ToString()}");
    }
}