using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenWrap.Commands.Cli
{
    // NOTE: This class is temporary, don't rely on it, it will change
    public class ConsoleCommandExecutor
    {
        readonly ICommandRepository _commands;
        readonly IEnumerable<ICommandLocator> _handlers;

        public ConsoleCommandExecutor(ICommandRepository commands, IEnumerable<ICommandLocator> handlers)
        {
            _commands = commands;
            _handlers = handlers;
        }
        public void Execute(string commandLine)
        {
            string commandParameters = commandLine;
            var command = _handlers.Select(x => x.Execute(ref commandParameters)).FirstOrDefault();
            if (command == null)
            {
                WriteError("The term '{0}' is not a recognized command or alias. Check the spelling or enter 'get-help' to get a list of available commands.");
                return;
            }
            foreach (var output in new CommandRunner().Run(command, commandParameters))
            {

            }
            ;
        }

        void WriteError(string message, params string[] args)
        {
            using (ColoredText.Red)
                Console.WriteLine(message);
        }
    }
}