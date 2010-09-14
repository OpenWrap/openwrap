using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using OpenFileSystem.IO.FileSystem.Local;
using OpenRasta.Client;
using OpenWrap.Commands;
using OpenWrap.Configuration;
using OpenWrap.Exports;
using OpenFileSystem.IO;
using OpenWrap.Repositories;
using OpenWrap.Resolvers;
using OpenWrap.Services;
using OpenWrap.Tasks;

namespace OpenWrap
{
    public static class ConsoleRunner
    {

        public static int Main(string[] args)
        {
            WrapServices.RegisterService<RuntimeAssemblyResolver>(new RuntimeAssemblyResolver());
            WrapServices.TryRegisterService<IFileSystem>(() => LocalFileSystem.Instance);
            WrapServices.TryRegisterService<IConfigurationManager>(() => new ConfigurationManager(WrapServices.GetService<IFileSystem>().GetDirectory(InstallationPaths.ConfigurationDirectory)));
            WrapServices.TryRegisterService<IEnvironment>(() => new CurrentDirectoryEnvironment());

            WrapServices.TryRegisterService<IPackageManager>(() => new PackageManager());
            WrapServices.RegisterService<ITaskManager>(new TaskManager());

            var commands = ReadCommands(WrapServices.GetService<IEnvironment>());
            var repo = new CommandRepository(commands);

            WrapServices.TryRegisterService<ICommandRepository>(() => repo);
            var processor = new CommandLineProcessor(repo);
            var backedupConsoleColor = Console.ForegroundColor;
            var returnCode = 0;
            foreach (var commandOutput in AllOutputs(processor, args).Where(x => x != null))
            {
                try
                {
                    if (HiddenVerboseOutput(args, commandOutput))
                        continue;
                    SetCommandColor(commandOutput.Type);
                    if (!commandOutput.Success)
                    {
                        returnCode = -1;
                    }
                    RenderOutput(commandOutput);
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

        static void LoadBootstrapAssemblies()
        {
            
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
                progress.StatusChanged += (s, e) =>
                {
                    Console.WriteLine(e.Message);
                };
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

        static bool HiddenVerboseOutput(string[] args, ICommandOutput commandOutput)
        {
            return commandOutput.Type == CommandResultType.Verbose && !HasFlag(args, "verbose");
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

        static IEnumerable<ICommandOutput> AllOutputs(CommandLineProcessor processor, string[] args)
        {
            return processor.Execute(args);
            var eventListener = WrapServices.GetService<ITaskManager>().GetListener();
            return Wrap(processor.Execute(args), eventListener).Merge(eventListener.Start().Select(x => ProgressMessage(x)));
        }

        static ICommandOutput ProgressMessage(ITask task)
        {
            return new TaskProgressMessage(task);
        }

        static IEnumerable<ICommandOutput> Wrap(IEnumerable<ICommandOutput> execute, ITaskListener eventListener)
        {
            foreach (var value in execute)
                yield return value;
            eventListener.Stop();
        }

        //static IEnumerable<ICommandResult> Read
        static IEnumerable<ICommandOutput> ReadTasks()
        {
            return null;
        }

        static IEnumerable<ICommandDescriptor> ReadCommands(IEnvironment environment)
        {
            return WrapServices.GetService<IPackageManager>()
                .GetExports<IExport>("commands", environment.ExecutionEnvironment, new[] { environment.ProjectRepository, environment.SystemRepository }.NotNull())
                .SelectMany(x => x.Items)
                .OfType<ICommandExportItem>()
                .Select(x => x.Descriptor).ToList();
        }
    }

    internal class TaskProgressMessage : ICommandOutput, IProgressOutput
    {
        readonly ITask _task;
        public event EventHandler<ProgressEventArgs> ProgressChanged;
        public event EventHandler<StatusChangedEventArgs> StatusChanged;
        public event EventHandler Complete;

        public TaskProgressMessage(ITask task)
        {
            _task = task;
            _task.ProgressChanged += (s, e) => this.ProgressChanged.Raise(this, e);
            _task.StatusChanged += (s, e) => StatusChanged.Raise(this, e);

        }

        public bool Success
        {
            get { return true; }
        }

        public ICommand Source
        {
            get { throw new NotImplementedException(); }
        }

        public CommandResultType Type
        {
            get { return CommandResultType.Info; }
        }
    }

    public interface IProgressOutput
    {
        event EventHandler<ProgressEventArgs> ProgressChanged;
        event EventHandler<StatusChangedEventArgs> StatusChanged;
        event EventHandler Complete;
    }
}