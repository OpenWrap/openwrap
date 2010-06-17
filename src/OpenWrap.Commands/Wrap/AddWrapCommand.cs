using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OpenWrap.Build.Services;
using OpenWrap.Commands.Core;
using OpenWrap.Dependencies;
using OpenWrap.IO;
using OpenWrap.Repositories;

namespace OpenWrap.Commands.Wrap
{
    [Command(Verb = "add", Noun = "wrap")]
    public class AddWrapCommand : ICommand
    {
        IEnvironment _environment;
        IFileSystem _fileSystem;
        IPackageManager _packageManager;

        [CommandInput(IsRequired = true, Position = 0)]
        public string Name { get; set; }

        [CommandInput(Position = 1)]
        public string Version { get; set; }

        [CommandInput]
        public bool LocalOnly { get; set; }

        [CommandInput]
        public bool SystemOnly { get; set; }



        public IEnumerable<ICommandResult> Execute()
        {
            _environment = WrapServices.GetService<IEnvironment>();
            _fileSystem = WrapServices.GetService<IFileSystem>();
            _packageManager = WrapServices.GetService<IPackageManager>();

            yield return VerifyWrapFile();
            yield return VeryfyWrapRepository();
            if (_environment.Descriptor != null)
            {
                yield return AddInstructionToWrapFile();
                foreach (var nestedResult in SyncWrapFileWithWrapDirectory())
                    yield return nestedResult;
            }
            else
            {
                var resolvedDependencies = _packageManager.TryResolveDependencies(GetDescriptor(), new[]{ _environment.SystemRepository}.Concat(_environment.RemoteRepositories));

                if (!resolvedDependencies.IsSuccess)
                {
                    yield return new GenericError("The dependency couldn't be found.");
                    yield break;
                }
                foreach (var dependency in resolvedDependencies.Dependencies)
                {
                    yield return new Result("Copying {0} to user repository.", dependency.Package.FullName);
                    using (var packageStream = dependency.Package.Load().OpenStream())
                    {
                        _environment.SystemRepository.Publish(dependency.Package.FullName, packageStream);
                        yield return new Result("Done.");
                    }
                }
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
                            VersionVertices = WrapDependencyParser.ParseVersions(Version.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)).ToList()
                        }
                    }
            };
        }

        ICommandResult AddInstructionToWrapFile()
        {
            // TODO: Make the environment descriptor separate from reader/writer,
            // and remove the File property on it.
            var dependLine = GetDependsLine();
            using (var fileStream = _environment.Descriptor.File.OpenWrite())
            using (var textWriter = new StreamWriter(fileStream, Encoding.UTF8))
                textWriter.WriteLine("\r\n" + dependLine);
            new WrapDependencyParser().Parse(dependLine, _environment.Descriptor);
            return null;
        }

        string GetDependsLine()
        {
            return "depends " + Name + " " + (Version ?? string.Empty);
        }

        IEnumerable<ICommandResult> SyncWrapFileWithWrapDirectory()
        {
            return new SyncWrapCommand().Execute();
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
                       : new GenericMessage("Project rep[ository not found, installing to system repository.");
        }
    }
}