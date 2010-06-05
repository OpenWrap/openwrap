using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Build.Services;
using OpenWrap.Commands;
using OpenWrap.Commands.Core;
using OpenWrap.Commands.Wrap;
using OpenWrap.Repositories;

namespace OpenWrap.Console
{
    internal class Program
    {
        static IEnumerable<IPackageInfo> GetLatestModules(IPackageRepository rep)
        {
            return from moduleName in rep.PackagesByName
                   from module in moduleName
                   orderby module.Version descending
                   select module;
        }

        static void Main(string[] args)
        {
            WrapServices.TryRegisterService<IEnvironment>(() => new CurrentDirectoryEnvironment());
            WrapServices.TryRegisterService<IPackageManager>(() => new PackageManager());

            var commands = ReadCommands(WrapServices.GetService<IEnvironment>());
            var repo = new CommandRepository(commands);

            WrapServices.TryRegisterService<ICommandRepository>(() => repo);
            var processor = new CommandLineProcessor(repo);
            var backedupConsoleColor = System.Console.ForegroundColor;
            foreach (var commandResult in processor.Execute(args).Where(x => x != null))
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
            System.Console.ReadLine();
        }

        static IEnumerable<ICommandDescriptor> ReadCommands(IEnvironment environment)
        {
            var packages = GetLatestModules(environment.UserRepository);
            if (environment.ProjectRepository != null)
                packages = packages.Concat(GetLatestModules(environment.ProjectRepository));

            var results = packages.ToLookup(x => x.Name).Select(x => x.FirstOrDefault());
            return results.SelectMany(x => x.Load().GetExport("commands", environment.ExecutionEnvironment).Items)
                .OfType<ICommandExportItem>()
                .Select(x => x.Descriptor)
                .ToList();
        }
    }
}