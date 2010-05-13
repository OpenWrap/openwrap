using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenRasta.Wrap.Commands;
using OpenRasta.Wrap.Console;
using OpenWrap.Commands.Wrap;

namespace OpenWrap.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var repo = new CommandRepository
            {
                new AttributeBasedCommandDescriptor<AddWrapCommand>()
            };
            var processor = new CommandLineProcessor(repo);
            bool isSuccess;
            var backedupConsoleColor = System.Console.ForegroundColor;
            try
            {
                var commandResult = processor.Execute(args);
                isSuccess = commandResult.Success;
                if (!isSuccess)
                    System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine(commandResult);
            }
            finally
            {
                System.Console.ForegroundColor = backedupConsoleColor;
            }
        }
    }
}
