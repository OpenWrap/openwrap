using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.Local;
using OpenWrap.Configuration;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.AssemblyResolvers;
using OpenWrap.PackageManagement.DependencyResolvers;
using OpenWrap.PackageManagement.Deployers;
using OpenWrap.PackageManagement.Exporters;
using OpenWrap.Runtime;
using OpenWrap.Tasks;

namespace OpenWrap.Commands.Cli
{
    public static class CliConsoleRunner
    {
        public static int Main(string[] args)
        {
            Console.WriteLine("Your version of the shell is out of date. Your commands will still work for now, but it is recommended that you update now.");
            return Main(StripExecutableName(Environment.CommandLine));
        }

        static string StripExecutableName(string commandLine)
        {
            return commandLine;
        }

        public static int Main(string argumentsLine)
        {
            throw new NotImplementedException();
            //Services.ServiceLocator.RegisterService(new RuntimeAssemblyResolver());
            //Services.ServiceLocator.TryRegisterService<IFileSystem>(() => LocalFileSystem.Instance);
            //Services.ServiceLocator.TryRegisterService<IConfigurationManager>(
            //        () => new DefaultConfigurationManager(Services.ServiceLocator.GetService<IFileSystem>().GetDirectory(DefaultInstallationPaths.ConfigurationDirectory)));
            //Services.ServiceLocator.TryRegisterService<IEnvironment>(() => new CurrentDirectoryEnvironment());

            //Services.ServiceLocator.TryRegisterService<IPackageResolver>(() => new ExhaustiveResolver());
            //Services.ServiceLocator.TryRegisterService<IPackageExporter>(() => new DefaultPackageExporter());
            //Services.ServiceLocator.TryRegisterService<IPackageDeployer>(() => new DefaultPackageDeployer());
            //Services.ServiceLocator.TryRegisterService<IPackageManager>(() => new DefaultPackageManager(
            //                                                                    Services.ServiceLocator.GetService<IPackageDeployer>(),
            //                                                                    Services.ServiceLocator.GetService<IPackageResolver>()
            //                                                                    ));

            //Services.ServiceLocator.RegisterService<ITaskManager>(new TaskManager());

            //var commands = Services.ServiceLocator.GetService<IEnvironment>().Commands();
            //var repo = new CommandRepository(commands);

            //Services.ServiceLocator.TryRegisterService<ICommandRepository>(() => repo);
            //var processor = new CommandLineProcessor(repo);
            //var backedupConsoleColor = Console.ForegroundColor;
            //var returnCode = 0;
            //foreach (var commandOutput in AllOutputs(processor, args).Where(x => x != null))
            //{
            //    try
            //    {
            //        if (HiddenVerboseOutput(args, commandOutput))
            //            continue;
            //        SetCommandColor(commandOutput.Type);
            //        RenderOutput(commandOutput);
            //        if (commandOutput.Type == CommandResultType.Error)
            //        {
            //            returnCode = -1;
            //        }
            //    }
            //    finally
            //    {
            //        Console.ForegroundColor = backedupConsoleColor;
            //    }
            //}
            //if (Debugger.IsAttached)
            //{
            //    Console.WriteLine("Press any key to continue...");
            //    Console.ReadKey();
            //}
            //return returnCode;
        }

        static IEnumerable<ICommandOutput> AllOutputs(CommandLineProcessor processor, string[] args)
        {
            return processor.Execute(args);
            //var eventListener = Services.Services.GetService<ITaskManager>().GetListener();
            //return Wrap(processor.Execute(args), eventListener).Merge(eventListener.Start().Select(x => ProgressMessage(x)));
        }

        static bool HasFlag(IEnumerable<string> args, string flag)
        {
            if (!flag.StartsWith("-"))
                flag = "-" + flag;
            for (var i = flag.Length; i >= 0; i--)
            {
                if (args.Contains(flag.Substring(i)))
                    return true;
            }
            return false;
        }

        static bool HiddenVerboseOutput(IEnumerable<string> args, ICommandOutput commandOutput)
        {
            return commandOutput.Type == CommandResultType.Verbose && !HasFlag(args, "verbose");
        }

        static ICommandOutput ProgressMessage(ITask task)
        {
            return new TaskProgressMessage(task);
        }

        static IEnumerable<ICommandOutput> ReadTasks()
        {
            return null;
        }

        static void RenderOutput(ICommandOutput commandOutput)
        {
            if (commandOutput is IProgressOutput)
            {
                var progress = (IProgressOutput)commandOutput;
                var progressFinished = new ManualResetEvent(false);
                int writtenDots = 0;
                bool progressOpened = false;
                Console.WriteLine(commandOutput);
                progress.StatusChanged += (s, e) => Console.WriteLine(e.Message);
                progress.ProgressChanged += (s, e) =>
                {
                    if (!progressOpened)
                    {
                        Console.Write("[");
                        progressOpened = true;
                    }
                    var currentDots = e.Progress / 10;
                    if (currentDots > writtenDots)
                    {
                        var dotsToWrite = currentDots - writtenDots;
                        Console.Write(new string('.', dotsToWrite));
                        writtenDots = currentDots;
                    }
                    if (e.Progress >= 100)
                    {
                        Console.WriteLine("]");
                    }
                };
                progress.Complete += (s, e) => progressFinished.Set();
                progressFinished.WaitOne();
            }
            else
                Console.WriteLine(commandOutput);
        }

        static void SetCommandColor(CommandResultType type)
        {
            switch (type)
            {
                case CommandResultType.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case CommandResultType.Verbose:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case CommandResultType.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
            }
        }

        static IEnumerable<ICommandOutput> Wrap(IEnumerable<ICommandOutput> execute, ITaskListener eventListener)
        {
            foreach (var value in execute)
                yield return value;
            eventListener.Stop();
        }
    }
}