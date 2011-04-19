using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using OpenFileSystem.IO;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.Local;
using OpenWrap.Collections;
using OpenWrap.Commands;
using OpenWrap.Commands.Cli;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.Exporters;
using OpenWrap.Repositories;
using OpenWrap.Runtime;
using OpenWrap.Services;

namespace OpenWrap.Build.Tasks
{
    public class RunCommand : Task
    {
        IFileSystem _fileSystem;
        bool _success;

        public RunCommand()
                : this(LocalFileSystem.Instance)
        {
        }

        public RunCommand(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        
        public string Args { get; set; }

        public string CurrentDirectory { get; set; }
        public bool Debug { get; set; }

        [Required]
        public string Noun { get; set; }

        [Required]
        public string Verb { get; set; }

        public override bool Execute()
        {
            if (Debug) Debugger.Launch();

            _success = true;

            throw new NotImplementedException();
            //var commandProcessor = new CommandLineProcessor(new CommandRepository(ReadCommands(Services.ServiceLocator.GetService<IEnvironment>())));

            //foreach (var cmd in commandProcessor.Execute(new[] { Noun, Verb }.Concat(GetArguments()).ToList()))
            //    ProcessOutput(cmd);

            return _success;
        }

        static IEnumerable<ICommandDescriptor> ReadCommands(IEnvironment environment)
        {
            throw new NotImplementedException();
            //return Services.ServiceLocator.GetService<IPackageExporter>()
            //        .GetExports<IExport>("commands", environment.ExecutionEnvironment, new[] { environment.ProjectRepository, environment.SystemRepository }.NotNull())
            //        .SelectMany(x => x.Items)
            //        .OfType<ICommandExportItem>()
            //        .Select(x => x.Descriptor).ToList();
        }

        IEnumerable<string> GetArguments()
        {
            var xmlDoc = "<parameters>" + Args + "</parameters>";
            foreach (var child in XDocument.Parse(xmlDoc).Root.Descendants())
            {
                yield return "-" + child.Name.LocalName;
                if (!child.IsEmpty)
                    yield return child.Value;
            }
        }

        void ProcessOutput(ICommandOutput cmd)
        {
            switch (cmd.Type)
            {
                case CommandResultType.Error:
                    Log.LogError(cmd.ToString());
                    _success = false;
                    break;
                case CommandResultType.Warning:
                    Log.LogWarning(cmd.ToString());
                    break;
                case CommandResultType.Data:
                case CommandResultType.Info:
                    Log.LogMessage(cmd.ToString());
                    break;
                case CommandResultType.Verbose:
                    Log.LogMessage(MessageImportance.Low, cmd.ToString());
                    break;
            }
        }
    }
}