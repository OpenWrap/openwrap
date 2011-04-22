using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.Local;
using OpenWrap.Commands.Cli.Locators;
using OpenWrap.Configuration;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.AssemblyResolvers;
using OpenWrap.PackageManagement.DependencyResolvers;
using OpenWrap.PackageManagement.Deployers;
using OpenWrap.PackageManagement.Exporters;
using OpenWrap.PackageManagement.Exporters.Assemblies;
using OpenWrap.PackageManagement.Exporters.Commands;
using OpenWrap.Runtime;
using OpenWrap.Services;
using OpenWrap.Tasks;

namespace OpenWrap.Commands.Cli
{
    /// <summary>
    /// The entrypoint used by the shell to execute code.
    /// </summary>
    public static class ShellRunner
    {
        public static int Main(string[] args)
        {
            Console.WriteLine("Your version of the shell is out of date. Your commands will still work for now, but it is recommended that you update now. The latest shell can be downloaded from http://openwrap.org");
            return Main(StripExecutableName(Environment.CommandLine));
        }

	    static string StripExecutableName(string commandLine)
	    {
            return commandLine.TrimStart().Substring(commandLine.IndexOf(commandLine[0] == '"' ? '"' : ' ', 1) + 1).TrimStart();
	    }
        public static int Main(string argumentsLine)
        {
            Services.ServiceLocator.RegisterService(new RuntimeAssemblyResolver());
            Services.ServiceLocator.TryRegisterService<IFileSystem>(() => LocalFileSystem.Instance);
            Services.ServiceLocator.TryRegisterService<IConfigurationManager>(
                    () => new DefaultConfigurationManager(Services.ServiceLocator.GetService<IFileSystem>().GetDirectory(DefaultInstallationPaths.ConfigurationDirectory)));
            Services.ServiceLocator.TryRegisterService<IEnvironment>(() => new CurrentDirectoryEnvironment());

            Services.ServiceLocator.TryRegisterService<IPackageResolver>(() => new ExhaustiveResolver());
            Services.ServiceLocator.TryRegisterService<IPackageExporter>(() => new DefaultPackageExporter(new List<IExportProvider>() { new EnvironmentDependentAssemblyExporter(ServiceLocator.GetService<IEnvironment>().ExecutionEnvironment), new CecilCommandExporter(ServiceLocator.GetService<IEnvironment>()) }));
            Services.ServiceLocator.TryRegisterService<IPackageDeployer>(() => new DefaultPackageDeployer());
            Services.ServiceLocator.TryRegisterService<IPackageManager>(() => new DefaultPackageManager(
                                                                                Services.ServiceLocator.GetService<IPackageDeployer>(),
                                                                                Services.ServiceLocator.GetService<IPackageResolver>(),
                                                                                Services.ServiceLocator.GetService<IPackageExporter>()
                                                                                ));

            Services.ServiceLocator.RegisterService<ITaskManager>(new TaskManager());


            var commands = Services.ServiceLocator.GetService<IPackageManager>().CommandExports(ServiceLocator.GetService<IEnvironment>());

            var commandRepository = new CommandRepository(commands.SelectMany(x=>x).Select(x=>x.Descriptor));
            Services.ServiceLocator.TryRegisterService<ICommandRepository>(() => commandRepository);

            return new ConsoleCommandExecutor(ServiceLocator.GetService<ICommandRepository>(),
                                       new List<ICommandLocator> { new NounVerbCommandLocator(commandRepository), new VerbNounCommandLocator(commandRepository) }).Execute(argumentsLine);
        }
    }
}