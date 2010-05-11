using System;
using System.Collections.Generic;
using OpenRasta.Wrap.Commands;

namespace OpenRasta.Wrap.Console
{
    //public class HelpCommand : ICommand
    //{
    //    private readonly IDictionary<string, Func<ICommand>> _commands;

    //    public HelpCommand(IDictionary<string, Func<ICommand>> commands)
    //    {
    //        _commands = commands;
    //    }

    //    public ICommandResult WrapAssembliesUpdated(string commandName, IEnumerable<string> commandArguments)
    //    {
    //        if (string.Compare(commandName, "commands", StringComparison.OrdinalIgnoreCase) == 0 || commandName == null)
    //            return new CommandList(_commands.Keys);
    //        return new UnknownCommand(commandName + " (help)", TODO);
    //    }
    //}
}