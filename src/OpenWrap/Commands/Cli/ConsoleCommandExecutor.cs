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
        public int Execute(string commandLine, IEnumerable<string> optionalInputs)
        {
            commandLine = commandLine.Trim();
            if (commandLine == string.Empty) return 0;
            string commandParameters = commandLine;
            var command = _handlers.Select(x => x.Execute(ref commandParameters)).Where(x=>x != null).FirstOrDefault();
            if (command == null)
            {
                var sp = commandLine.IndexOf(" ");

                WriteError("The term '{0}' is not a recognized command or alias. Check the spelling or enter 'get-help' to get a list of available commands.", sp != -1 ? commandLine.Substring(0, sp) : commandLine);
                return -10;
            }
            int returnCode = 0;
            var commandLineRunner = new CommandLineRunner() { OptionalInputs = optionalInputs };
            foreach (var output in commandLineRunner.Run(command, commandParameters))
            {
                using (ColorFromOutput(output))
                    Console.WriteLine(output.ToString());
                if (output.Type == CommandResultType.Error)
                {
                    returnCode = -50;
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
                Console.WriteLine(message, args);
        }
    }
}