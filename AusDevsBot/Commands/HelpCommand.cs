using System.ComponentModel;
using Discord.Commands;

namespace AusDevsBot.Commands;

public class HelpCommand : ModuleBase<SocketCommandContext>
{

    [Command("help")]
    [Description("Displays the help menu")]
    public async Task Help()
    {
        const string helpMsg = """
                                ## Save snippet
                                `!snippet` - used in reply to a post, allows you to snippet a whole post
                                `/snippet {snippet} {language} {optional quick tag}`
                               
                                ## Fetch Snippets
                                `/my-snippets {page number}` - displays snippets with paging parameter
                                `/detail {id}` - prints the entire snippet with the corresponding id
                               
                                ## Stats
                                `/snippet-stats` - displays snippet stats
                               
                                ## Edit
                                `/rename {id} {new id}` - updates the quicksave id for a snippet
                                `/set-content {id} {new content}` - updates the snippet content
                                `!set-content {id}` - updates the content of the snippet with the corresponding id with the content in the replied message
                               
                                ## Delete
                                `/delete {id}` - delete the snippet with the corresponding id
                               """;
        
        await ReplyAsync(helpMsg);

    }
}