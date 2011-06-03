using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
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
    ///   The entrypoint used by the shell to execute code.
    /// </summary>
    public static class ShellRunner
    {
        public static int Main(string[] args)
        {
            Console.WriteLine(
                "Your version of the shell is out of date and cannot execute this version of OpenWrap.\r\nWe're very sorry for the invonvenience, but we promise the new version is eons better.\r\nThe latest shell can be downloaded from http://openwrap.org by click on the 'Download' icon on the top right.");
            return -250;
        }

        public static int Main(IDictionary<string, object> env)
        {
            ServiceLocator.TryRegisterService<IFileSystem>(() => LocalFileSystem.Instance);
            ServiceLocator.TryRegisterService<IConfigurationManager>(
                () => new DefaultConfigurationManager(ServiceLocator.GetService<IFileSystem>().GetDirectory(DefaultInstallationPaths.ConfigurationDirectory)));
            ServiceLocator.TryRegisterService<IEnvironment>(() =>
            {
                var cdenv = new CurrentDirectoryEnvironment(LocalFileSystem.Instance.GetDirectory(env.CurrentDirectory()));
                if (env.SysPath() != null)
                    cdenv.SystemRepositoryDirectory = LocalFileSystem.Instance.GetDirectory(new Path(env.SysPath()).Combine("wraps"));
                return cdenv;
            });
            ServiceLocator.TryRegisterService<IPackageResolver>(() => new ExhaustiveResolver());
            ServiceLocator.TryRegisterService<IPackageExporter>(() => new DefaultPackageExporter(new List<IExportProvider>
            {
                new DefaultAssemblyExporter(),
                new CecilCommandExporter()
            }));
            ServiceLocator.TryRegisterService<IPackageDeployer>(() => new DefaultPackageDeployer());
            ServiceLocator.TryRegisterService<IPackageManager>(() => new DefaultPackageManager(
                                                                         ServiceLocator.GetService<IPackageDeployer>(),
                                                                         ServiceLocator.GetService<IPackageResolver>(),
                                                                         ServiceLocator.GetService<IPackageExporter>()
                                                                         ));

            ServiceLocator.RegisterService<ITaskManager>(new TaskManager());
            ServiceLocator.TryRegisterService<IAssemblyResolver>(() => new CecilStaticAssemblyResolver(ServiceLocator.GetService<IPackageManager>(), ServiceLocator.GetService<IEnvironment>()));


            var commands = ServiceLocator.GetService<IPackageManager>().CommandExports(ServiceLocator.GetService<IEnvironment>());

            var commandRepository = new CommandRepository(commands.SelectMany(x => x).Select(x => x.Descriptor));
            ServiceLocator.TryRegisterService<ICommandRepository>(() => commandRepository);

            ServiceLocator.RegisterService(new RuntimeAssemblyResolver());
            return new ConsoleCommandExecutor(new List<ICommandLocator>
                                              {
                                                  new NounVerbCommandLocator(commandRepository),
                                                  new VerbNounCommandLocator(commandRepository),
                                                  new DefaultVerbCommandLocator(commandRepository)
                                              }).Execute(env.CommandLine(), env.ShellArgs());
        }
    }
}