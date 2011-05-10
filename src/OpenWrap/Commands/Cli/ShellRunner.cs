using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// The entrypoint used by the shell to execute code.
    /// </summary>
    public static class ShellRunner
    {
        public static int Main(string[] args)
        {
            Console.WriteLine("Your version of the shell is out of date and cannot execute this version of OpenWrap.\r\nWe're very sorry for the invonvenience, but we promise the new version is eons better.\r\nThe latest shell can be downloaded from http://openwrap.org by click on the 'Download' icon on the top right.");
            return -250;
        }

        public static int Main(IDictionary<string, object> env)
        {
            Services.ServiceLocator.TryRegisterService<IFileSystem>(() => LocalFileSystem.Instance);
            Services.ServiceLocator.TryRegisterService<IConfigurationManager>(
                    () => new DefaultConfigurationManager(Services.ServiceLocator.GetService<IFileSystem>().GetDirectory(DefaultInstallationPaths.ConfigurationDirectory)));
            Services.ServiceLocator.TryRegisterService<IEnvironment>(() =>
            {
                var cdenv = new CurrentDirectoryEnvironment(LocalFileSystem.Instance.GetDirectory(env.CurrentDirectory()));
                if (env.SysPath() != null)
                    cdenv.SystemRepositoryDirectory = LocalFileSystem.Instance.GetDirectory(new Path(env.SysPath()).Combine("wraps"));
                return cdenv;
            });
            Services.ServiceLocator.TryRegisterService<IPackageResolver>(() => new ExhaustiveResolver());
            Services.ServiceLocator.TryRegisterService<IPackageExporter>(() => new DefaultPackageExporter(new List<IExportProvider>()
            {
                    new DefaultAssemblyExporter(),
                    new CecilCommandExporter()
            }));
            Services.ServiceLocator.TryRegisterService<IPackageDeployer>(() => new DefaultPackageDeployer());
            Services.ServiceLocator.TryRegisterService<IPackageManager>(() => new DefaultPackageManager(
                                                                                Services.ServiceLocator.GetService<IPackageDeployer>(),
                                                                                Services.ServiceLocator.GetService<IPackageResolver>(),
                                                                                Services.ServiceLocator.GetService<IPackageExporter>()
                                                                                ));

            Services.ServiceLocator.RegisterService<ITaskManager>(new TaskManager());
            ServiceLocator.TryRegisterService<IAssemblyResolver>(() => new CecilStaticAssemblyResolver(ServiceLocator.GetService<IPackageManager>(), ServiceLocator.GetService<IEnvironment>()));


            var commands = Services.ServiceLocator.GetService<IPackageManager>().CommandExports(ServiceLocator.GetService<IEnvironment>());

            var commandRepository = new CommandRepository(commands.SelectMany(x=>x).Select(x=>x.Descriptor));
            Services.ServiceLocator.TryRegisterService<ICommandRepository>(() => commandRepository);

            Services.ServiceLocator.RegisterService(new RuntimeAssemblyResolver());
            return new ConsoleCommandExecutor(ServiceLocator.GetService<ICommandRepository>(),
                                       new List<ICommandLocator>
                                       {
                                               new NounVerbCommandLocator(commandRepository), 
                                               new VerbNounCommandLocator(commandRepository),
                                               new DefaultVerbCommandLocator(commandRepository)
                                       }).Execute(env.CommandLine(), env.ShellArgs());
        }
    }

    public class CecilStaticAssemblyResolver : IAssemblyResolver
    {
        readonly IPackageManager _packageManager;
        readonly IEnvironment _environment;
        IEnumerable<Exports.IAssembly> _allAssemblies;

        public CecilStaticAssemblyResolver(IPackageManager packageManager, IEnvironment environment)
        {
            _packageManager = packageManager;
            _environment = environment;
            var allAssemblies = Enumerable.Empty<Exports.IAssembly>();

            if (_environment.ProjectRepository != null)
                allAssemblies = _packageManager.GetProjectAssemblyReferences(_environment.Descriptor, _environment.ProjectRepository, environment.ExecutionEnvironment, true);
            var selectedPackages = allAssemblies.Select(x => x.Package.Name).ToList();
            var systemAssemblies = _packageManager.GetSystemExports<Exports.IAssembly>(_environment.SystemRepository, environment.ExecutionEnvironment)
                .Where(x => selectedPackages.Contains(x.Key) == false)
                .SelectMany(x => x);
            _allAssemblies = allAssemblies.Concat(systemAssemblies).ToList();
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            return ReadAssembly(name.Name, ReadingMode.Deferred);
        }

        AssemblyDefinition ReadAssembly(string name, ReadingMode readingMode)
        {
            var matchingAssembly = _allAssemblies.FirstOrDefault(x => x.AssemblyName.Name == name);
            return matchingAssembly != null ? AssemblyDefinition.ReadAssembly(matchingAssembly.File.Path.FullPath, new ReaderParameters(readingMode) { AssemblyResolver = this}) : null;
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            return ReadAssembly(name.Name, parameters.ReadingMode);
        }

        public AssemblyDefinition Resolve(string fullName)
        {
            return Resolve(fullName, new ReaderParameters(ReadingMode.Deferred));
        }

        public AssemblyDefinition Resolve(string fullName, ReaderParameters parameters)
        {
            var assemblyName = fullName.IndexOf(',') != -1 ? fullName.Substring(0, fullName.IndexOf(',')) : fullName;
            return ReadAssembly(assemblyName, parameters.ReadingMode);
        }
    }

    public static class DictionaryExtensions
    {
        const string SYSPATH = "openwrap.syspath";
        const string CD = "openwrap.cd";
        const string SHELL_COMMAND_LINE = "openwrap.shell.commandline";
        const string SHELL_ARGS = "openwrap.shell.args";
        public static string SysPath(this IDictionary<string, object> env)
        {
            return env.ContainsKey(SYSPATH) ? env[SYSPATH] as string : null;
        }
        public static string CurrentDirectory(this IDictionary<string, object> env)
        {
            return env.ContainsKey(CD) ? env[CD] as string ?? Environment.CurrentDirectory : Environment.CurrentDirectory;
        }
        public static IEnumerable<string> ShellArgs(this IDictionary<string, object> env)
        {
            return env.ContainsKey(SHELL_ARGS) ? env[SHELL_ARGS] as IEnumerable<string> : Enumerable.Empty<string>();
        }
        public static string CommandLine(this IDictionary<string, object> env)
        {
            return env.ContainsKey(SHELL_COMMAND_LINE) ? env[SHELL_COMMAND_LINE] as string : null;

        }
    }
}