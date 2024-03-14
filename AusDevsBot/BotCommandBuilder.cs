using System.Collections.Immutable;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace AusDevsBot;

public class BotCommandBuilder
{
    private SocketGuild _guild;

    private Dictionary<string, Func<SocketSlashCommand, Task>> _registeredCommands = new();


    public BotCommandBuilder() { }

    public void WithGuild(SocketGuild guild)
    {
        _guild = guild;
    }


    public async Task AddCommand(string name, string description, Func<SocketSlashCommand, Task> commandHandler, params SlashCommandOptionBuilder[] options)
    {
        if (_guild == null)
        {
            throw new ArgumentNullException("No guild has been registered");
        }
        
        var command = new SlashCommandBuilder()
            .WithName(name)
            .WithDescription(description)
            .AddOptions(options);
        

        try
        {
            await _guild.CreateApplicationCommandAsync(command.Build());
            _registeredCommands.Add(name, commandHandler);
        }
        catch (HttpException e)
        {
            var json = JsonConvert.SerializeObject(e.Errors, Formatting.Indented);
            
            // todo: logging
            Console.WriteLine(json);
        }
    }

    public async Task HandleIncomingSlashCommands(SocketSlashCommand command)
    {
        var name = command.Data.Name;
        if (!_registeredCommands.ContainsKey(name))
        {
            await command.RespondAsync($"No registered command matches '{name}'");
            return;
        }

        try
        {
            await _registeredCommands[name].Invoke(command);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Looks like the command failed to execute and threw an exception\n {e}");
            await command.RespondAsync("Oops, looks like im having trouble with that command");
        }
        
        
    }

}