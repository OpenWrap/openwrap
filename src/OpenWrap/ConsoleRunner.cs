using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenWrap.Commands;
using OpenWrap.Exports;
using OpenWrap.IO;
using OpenWrap.Repositories;
using OpenWrap.Resolvers;
using OpenWrap.Services;

namespace OpenWrap
{
    public static class ConsoleRunner
    {

        public static int Main(string[] args)
        {
            WrapServices.TryRegisterService<IEnvironment>(() => new CurrentDirectoryEnvironment());
            WrapServices.TryRegisterService<IPackageManager>(() => new PackageManager());
            WrapServices.RegisterService<RuntimeAssemblyResolver>(new RuntimeAssemblyResolver());
            WrapServices.TryRegisterService<IFileSystem>(()=>FileSystems.Local);
            var commands = ReadCommands(WrapServices.GetService<IEnvironment>());
            var repo = new CommandRepository(commands);

            WrapServices.TryRegisterService<ICommandRepository>(() => repo);
            var processor = new CommandLineProcessor(repo);
            var backedupConsoleColor = Console.ForegroundColor;
            var returnCode = 0;
            foreach (var commandResult in processor.Execute(args).Where(x => x != null))
            {
                try
                {
                    if (!commandResult.Success)
                    {
                        returnCode = -1;
                        Console.ForegroundColor = ConsoleColor.Red;
                    }
                    Console.WriteLine(commandResult);
                }
                finally
                {
                    Console.ForegroundColor = backedupConsoleColor;
                }
            }
            return returnCode;
        }

        static IEnumerable<ICommandDescriptor> ReadCommands(IEnvironment environment)
        {
            return WrapServices.GetService<IPackageManager>()
                .GetExports<IExport>("commands", environment.ExecutionEnvironment, new[]{ environment.ProjectRepository, environment.SystemRepository})
                .SelectMany(x=>x.Items)
                .OfType<ICommandExportItem>()
                .Select(x=>x.Descriptor).ToList();
        }
    }
}