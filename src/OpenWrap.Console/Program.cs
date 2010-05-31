using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Build.Services;
using OpenWrap.Commands;
using OpenWrap.Commands.Core;
using OpenWrap.Commands.Wrap;
using OpenWrap.Repositories;

namespace OpenWrap.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            WrapServices.TryRegisterService<IEnvironment>(() => new CurrentDirectoryEnvironment());
            WrapServices.TryRegisterService<IPackageManager>(() => new PackageManager());

            var repo = new CommandRepository
            {
                new AttributeBasedCommandDescriptor<AddWrapCommand>(),
                new AttributeBasedCommandDescriptor<HelpCommand>(),
                new AttributeBasedCommandDescriptor<SyncWrapCommand>()
            };
            WrapServices.TryRegisterService<ICommandRepository>(()=> repo);
            var processor = new CommandLineProcessor(repo);
            var backedupConsoleColor = System.Console.ForegroundColor;
            foreach (var commandResult in processor.Execute(args))
            {
                try
                {
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

}
