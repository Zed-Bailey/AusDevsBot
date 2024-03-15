using System.Text;
using AusDevsBot.Data;
using Discord.WebSocket;

namespace AusDevsBot.Commands.SlashCommands;

public class FetchSnippets
{

    /// <summary>
    /// Fetches snippets with paging functionality
    /// </summary>
    public class MySnippets : ISlashCommand
    {
        private BotDbContext _context;
        
        public MySnippets(BotDbContext context)
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

            const int perPage = 5;
            
            var page = ((long?) command.Data.Options.FirstOrDefault(x => x.Name == "page")?.Value) ?? 1;
            // minus 1 to zero index the page
            var skipAmount = Convert.ToInt32(page - 1) * perPage;
            var snippets = user.SavedSnippets.Skip(skipAmount).Take(perPage);
            
            var sb = new StringBuilder();
            sb.AppendLine(
                "+--------------------------------------+-------------------------------------+\n| id                                   | content                             |\n+--------------------------------------+-------------------------------------+");
            
            foreach (var snip in snippets)
            {
                var content = snip.Content.Replace('\n', ' ');
                if (content.Length > 30)
                {
                    content = content.Substring(0, 30);
                }
                // 36 is length of guid
                sb.AppendLine($"| {snip.QuickSaveId?.PadRight(36) ?? snip.SnippetId.ToString()} | {content.PadRight(30)}..... |");
                sb.AppendLine("+--------------------------------------+-------------------------------------+");
            }
            
            int pages = ((user.NumberOfSnippets - 1) / perPage) + 1;
            
            sb.AppendLine($"page {page}/{pages}");
                
            await command.RespondAsync($"```\n{sb}\n```");
        }
    }


    public class DetailSnippet : ISlashCommand
    {
        private BotDbContext _context;
        
        public DetailSnippet(BotDbContext context)
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

            var msg = $"""
                      ```
                      Id: {snippet.SnippetId}
                      Quick-save id: {snippet.QuickSaveId}
                      ---
                      {snippet.Content}
                      ---
                      Created at : {snippet.CreatedAt}
                      Last updated : {snippet.LastUpdated}
                      ```
                      """;

            await command.RespondAsync(msg);

        }
    }
    
    
}