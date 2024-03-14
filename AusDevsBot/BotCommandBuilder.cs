using System.Collections.Immutable;
using AusDevsBot.Data;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace AusDevsBot;

public class BotCommandBuilder
{
    private SocketGuild _guild;

    // private Dictionary<string, Func<SocketSlashCommand, Task>> _registeredCommands = new();
    private Dictionary<string, Type> _registeredCommands = new();

    private IServiceProvider _serviceProvider;

    public BotCommandBuilder(IServiceProvider provider)
    {
        _serviceProvider = provider;
    }

    public void WithGuild(SocketGuild guild)
    {
        _guild = guild;
    }


    // public async Task AddCommand(string name, string description, Func<SocketSlashCommand, Task> commandHandler, params SlashCommandOptionBuilder[] options)
    public async Task AddCommand<T>(string name, string description, params SlashCommandOptionBuilder[] options) where T : ISlashCommand
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
            // _registeredCommands.Add(name, commandHandler);
            _registeredCommands.Add(name, typeof(T));
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
            // create a new instance of the class
            // activator will inject dependencies into constructor from service provider
            var instance = (ISlashCommand) ActivatorUtilities.CreateInstance(_serviceProvider, _registeredCommands[name]);
            
            // handle command
            await instance.HandleCommand(command);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Looks like the command failed to execute and threw an exception\n {e}");
            await command.RespondAsync("Oops, looks like im having trouble with that command");
        }
        
        
    }

}