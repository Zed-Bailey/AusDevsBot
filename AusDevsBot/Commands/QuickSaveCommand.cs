using Discord.Commands;

namespace AusDevsBot.Commands;

public class QuickSaveCommand : ModuleBase<SocketCommandContext>
{

    [Command("save")]
    [Summary("Quick saves a message")]
    public async Task QuickSaveMessage(string? quickSaveId = null)
    {
        var replyMsg = Context.Message.ReferencedMessage;
        
        await ReplyAsync(replyMsg.Content);
    }
}