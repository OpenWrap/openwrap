using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Commands.Cli.Locators;

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
        public int Execute(string commandLine)
        {
            string commandParameters = commandLine;
            var command = _handlers.Select(x => x.Execute(ref commandParameters)).FirstOrDefault();
            if (command == null)
            {
                WriteError("The term '{0}' is not a recognized command or alias. Check the spelling or enter 'get-help' to get a list of available commands.");
                return -1;
            }
            int returnCode = 0;
            foreach (var output in new CommandLineRunner().Run(command, commandParameters))
            {
                using (ColorFromOutput(output))
                    Console.WriteLine(output.ToString());
                if (output.Type == CommandResultType.Error)
                {
                    returnCode = -1;
                }
            }
            return returnCode;
        }

        IDisposable ColorFromOutput(ICommandOutput output)
        {
            switch(output.Type)
            {
                case CommandResultType.Error: return ColoredText.Red;
                case CommandResultType.Warning: return ColoredText.Yellow;
                case CommandResultType.Verbose: return ColoredText.Gray;
                
            }
            return new ActionOnDispose(()=>{});
        }

        void WriteError(string message, params string[] args)
        {
            using (ColoredText.Red)
                Console.WriteLine(message);
        }
    }
}