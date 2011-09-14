using System;
using System.Collections.Generic;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.Local;
using OpenWrap.Commands.Cli.Locators;
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
            var serviceRegistry = new ServiceRegistry().Override<IEnvironment>(() =>
            {
                var cdenv = new CurrentDirectoryEnvironment(LocalFileSystem.Instance.GetDirectory(env.CurrentDirectory()));
                if (env.SysPath() != null)
                    cdenv.SystemRepositoryDirectory = LocalFileSystem.Instance.GetDirectory(new Path(env.SysPath()).Combine("wraps"));
                return cdenv;
            });
            var formatterType = env.Formatter();
            if (formatterType != null)
            {
                serviceRegistry.Override(() => (ICommandOutputFormatter)Activator.CreateInstance(Type.GetType(formatterType)));
            }
            serviceRegistry.Initialize();

            return new ConsoleCommandExecutor(ServiceLocator.GetService<IEnumerable<ICommandLocator>>(), ServiceLocator.GetService<IEventHub>(), ServiceLocator.GetService<ICommandOutputFormatter>())
                .Execute(env.CommandLine(), env.ShellArgs());
        }
#pragma warning restore 28
    }
}