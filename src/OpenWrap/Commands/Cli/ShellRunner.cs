using System;
using System.Collections.Generic;
using System.Linq;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.Local;
using OpenWrap.Commands.Cli.Locators;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.AssemblyResolvers;
using OpenWrap.Runtime;
using OpenWrap.Services;

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
#pragma warning disable 28
        public static int Main(IDictionary<string, object> env)
        {
            bool requireFirstRunOnProjectUpgrade = false;
            bool inSystem = true;
            var serviceRegistry = new ServiceRegistry();
            serviceRegistry = serviceRegistry.Override<IEnvironment>(() =>
            {
                var cdenv = new CurrentDirectoryEnvironment(LocalFileSystem.Instance.GetDirectory(env.CurrentDirectory()))
                {
                    BeforeProjectRepositoryInitialized = (dir, options) =>
                    {
                        requireFirstRunOnProjectUpgrade = dir.GetFile("packages").Exists == false;
                        inSystem = false;
                    }
                };
                if (env.SysPath() != null)
                    cdenv.SystemRepositoryDirectory = LocalFileSystem.Instance.GetDirectory(new Path(env.SysPath()).Combine("wraps"));

                return cdenv;
            });

            if (env.ShellArgs().ContainsNoCase("-UseSystem"))
            {
                
                serviceRegistry
                    .Override(() => new RuntimeAssemblyResolver
                    {
                        IgnoreProjectAssemblies = true
                    })
                    .Override<ICommandRepository>(() => new CommandRepository(
                                                            ServiceLocator.GetService<IPackageManager>().CommandExports(
                                                                ServiceLocator.GetService<IEnvironment>(), true)
                                                                .SelectMany(x => x)
                                                                .Select(x => x.Descriptor)));
            }
            var formatterType = env.Formatter();
            if (formatterType != null)
            {
                serviceRegistry.Override(() => (ICommandOutputFormatter)Activator.CreateInstance(Type.GetType(formatterType)));
            }
            serviceRegistry.Initialize();

            if (requireFirstRunOnProjectUpgrade)
            {
                ServiceLocator.GetService<ICommandOutputFormatter>().Render(
                    new Warning("This is the first time you run a version of OpenWrap that supports post-install hooks on this project. Please wait while we run the hooks for the current packages."));

                serviceRegistry.Initialize();
            }

            return new ConsoleCommandExecutor(ServiceLocator.GetService<IEnumerable<ICommandLocator>>(), ServiceLocator.GetService<IEventHub>(), ServiceLocator.GetService<ICommandOutputFormatter>())
                .Execute(env.CommandLine(), env.ShellArgs());
        }
#pragma warning restore 28
    }
}