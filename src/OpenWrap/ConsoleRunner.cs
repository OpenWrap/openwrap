using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenFileSystem.IO.FileSystem.Local;
using OpenWrap.Commands;
using OpenWrap.Configuration;
using OpenWrap.Exports;
using OpenFileSystem.IO;
using OpenWrap.Repositories;
using OpenWrap.Resolvers;
using OpenWrap.Services;

namespace OpenWrap
{
    public static class ConsoleRunner
    {

        public static int Main(string[] args)
        {
            WrapServices.TryRegisterService<IFileSystem>(()=>LocalFileSystem.Instance);
            WrapServices.TryRegisterService<IConfigurationManager>(()=>new ConfigurationManager(WrapServices.GetService<IFileSystem>().GetDirectory(InstallationPaths.ConfigurationDirectory)));
            WrapServices.TryRegisterService<IEnvironment>(() => new CurrentDirectoryEnvironment());

            WrapServices.TryRegisterService<IPackageManager>(() => new PackageManager());
            WrapServices.RegisterService<RuntimeAssemblyResolver>(new RuntimeAssemblyResolver());

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
            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
            return returnCode;
        }

        static IEnumerable<ICommandDescriptor> ReadCommands(IEnvironment environment)
        {
            return WrapServices.GetService<IPackageManager>()
                .GetExports<IExport>("commands", environment.ExecutionEnvironment, new[]{ environment.ProjectRepository, environment.SystemRepository}.NotNull())
                .SelectMany(x=>x.Items)
                .OfType<ICommandExportItem>()
                .Select(x=>x.Descriptor).ToList();
        }
    }
}