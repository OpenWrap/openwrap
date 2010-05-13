using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenRasta.Wrap.Build.Services;
using OpenRasta.Wrap.Commands;
using OpenRasta.Wrap.Console;
using OpenRasta.Wrap.Sources;
using OpenWrap.Commands.Wrap;

namespace OpenWrap.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            WrapServices.RegisterService<IEnvironment>(new CurrentDirectoryEnvironment());
            var repo = new CommandRepository
            {
                new AttributeBasedCommandDescriptor<AddWrapCommand>()
            };
            var processor = new CommandLineProcessor(repo);
            var backedupConsoleColor = System.Console.ForegroundColor;
            try
            {
                var commandResult = processor.Execute(args);
                if (!commandResult.Success)
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
