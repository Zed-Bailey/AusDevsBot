using AusDevsBot.Data;
using Discord;
using Discord.WebSocket;

namespace AusDevsBot.Commands.SlashCommands;

public class SaveSnippet: ISlashCommand
{
    private BotDbContext _context;

    public SaveSnippet(BotDbContext context)
    {
        _context = context;
    }
    
    public async Task HandleCommand(SocketSlashCommand command)
    {
        var content = (string) command.Data.Options.First(x => x.Name == "content").Value;
        var quickId = (string?) command.Data.Options.FirstOrDefault(x => x.Name == "quick-id")?.Value;
        
        var snippet = new SavedSnippet()
        {
            Content = content,
            QuickSaveId = quickId,
            Language = "text",
            LastUpdated = DateTime.UtcNow,
        };

        var user = await _context.Users.FindAsync(command.User.Id);
        if (user == null)
        {
            Console.WriteLine("Created a new user");
            // create a new user
            var newUser = new User()
            {
                UserId = command.User.Id,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            user = newUser;
        }
        else
        {
            Console.WriteLine("Found pre-exisiting user");
        }

        snippet.User = user;
        _context.SavedSnippets.Add(snippet);
        
        user.SavedSnippets.Add(snippet);
        await _context.SaveChangesAsync();
        
        await command.RespondAsync($"Saved snippet, id: {snippet.QuickSaveId ?? snippet.SnippetId.ToString()}");
        

    }
}