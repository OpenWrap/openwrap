using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OpenWrap.Dependencies;
using OpenWrap.IO;
using OpenWrap.Repositories;
using OpenWrap.Services;

namespace OpenWrap.Commands.Wrap
{
    [Command(Verb = "add", Noun = "wrap")]
    public class AddWrapCommand : ICommand
    {
        IEnvironment _environment;
        IPackageManager _packageManager;

        [CommandInput(IsRequired = true, Position = 0)]
        public string Name { get; set; }

        [CommandInput(Position = 1)]
        public string Version { get; set; }

        [CommandInput]
        public bool ProjectOnly { get; set; }

        [CommandInput]
        public bool SystemOnly { get; set; }



        public IEnumerable<ICommandResult> Execute()
        {
            _environment = WrapServices.GetService<IEnvironment>();
            WrapServices.GetService<IFileSystem>();
            _packageManager = WrapServices.GetService<IPackageManager>();

            yield return VerifyWrapFile();
            yield return VeryfyWrapRepository();

            if (_environment.Descriptor != null && !SystemOnly)
            {
                AddInstructionToWrapFile();
                foreach (var nestedResult in SyncWrapFileWithWrapDirectory())
                    yield return nestedResult;
            }
            else
            {
                var resolvedDependencies = _packageManager.TryResolveDependencies(GetDescriptor(), _environment.RepositoriesForRead());

                if (!resolvedDependencies.IsSuccess)
                {
                    yield return new GenericError("The dependency couldn't be found.");
                    yield break;
                }
                var repositoriesToCopyTo = _environment.RemoteRepositories.Concat(new[]
                {
                    _environment.CurrentDirectoryRepository,
                    ProjectOnly ? null : _environment.SystemRepository,
                    SystemOnly ? null : _environment.ProjectRepository
                });
                foreach(var msg in _packageManager.CopyPackagesToRepositories(resolvedDependencies, repositoriesToCopyTo))
                    yield return msg;
            }
        }

        WrapDescriptor GetDescriptor()
        {
            return new WrapDescriptor
            {
                Dependencies =
                    {
                        new WrapDependency
                        {
                            Name = Name,
                            VersionVertices = Version != null 
                                ? WrapDependencyParser.ParseVersions(Version.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)).ToList()
                                : new List<VersionVertice>()
                        }
                    }
            };
        }

        void AddInstructionToWrapFile()
        {

            // TODO: Make the environment descriptor separate from reader/writer,
            // and remove the File property on it.
            var dependLine = GetDependsLine();
            using (var fileStream = _environment.Descriptor.File.OpenWrite())
            using (var textWriter = new StreamWriter(fileStream, Encoding.UTF8))
                textWriter.WriteLine("\r\n" + dependLine);
            new WrapDependencyParser().Parse(dependLine, _environment.Descriptor);

        }

        string GetDependsLine()
        {
            return "depends " + Name + " " + (Version ?? string.Empty);
        }

        IEnumerable<ICommandResult> SyncWrapFileWithWrapDirectory()
        {
            return new SyncWrapCommand()
            {
                ProjectOnly = ProjectOnly,
                SystemOnly = SystemOnly
            }
                .Execute();
        }

        ICommandResult VerifyWrapFile()
        {
            return _environment.Descriptor != null
                ? new GenericMessage(@"No wrap descriptor found, installing locally.")
                : new GenericMessage("Wrap descriptor found.");
        }

        ICommandResult VeryfyWrapRepository()
        {
            return _environment.ProjectRepository != null
                       ? new GenericMessage("Project repository found.")
                       : new GenericMessage("Project repository not found, installing to system repository.");
        }
    }
}