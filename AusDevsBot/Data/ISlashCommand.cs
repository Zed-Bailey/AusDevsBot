using Discord.WebSocket;

namespace AusDevsBot.Data;

public interface ISlashCommand
{
    public Task HandleCommand(SocketSlashCommand command);
}