using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OpenWrap.Commands.Remote;
using OpenWrap.Dependencies;
using OpenWrap.Exports;
using OpenFileSystem.IO;
using OpenWrap.Repositories;
using OpenWrap.Services;

namespace OpenWrap.Commands.Wrap
{
    [Command(Verb = "add", Noun = "wrap")]
    public class AddWrapCommand : AbstractCommand
    {
        [CommandInput(IsRequired = true, Position = 0)]
        public string Name { get; set; }

        [CommandInput]
        public bool NoDescriptorUpdate { get; set; }

        [CommandInput]
        public bool ProjectOnly { get; set; }

        [CommandInput]
        public bool SystemOnly { get; set; }

        [CommandInput(Position = 1)]
        public string Version { get; set; }

        public IEnvironment Environment { get; set; }

        public IPackageManager PackageManager
        {
            get { return WrapServices.GetService<IPackageManager>(); }
        }

        bool ShouldUpdateDescriptor
        {
            get
            {
                return Environment.Descriptor != null &&
                       !NoDescriptorUpdate &&
                       !SystemOnly;
            }
        }

        public AddWrapCommand()
            : this(WrapServices.GetService<IEnvironment>())
        {
        }
        public AddWrapCommand(IEnvironment environment)
        {
            Environment = environment;
        }

        public override IEnumerable<ICommandOutput> Execute()
        {
            if (Name.EndsWith(".wrap", StringComparison.OrdinalIgnoreCase))
            {
                var originalName = Name;
                Name = WrapNameUtility.GetName(Name);
                Version = "= " + WrapNameUtility.GetVersion(originalName);
                yield return new GenericMessage(string.Format("The requested package contained '.wrap' in the name. Assuming you pointed to a file and meant a package named '{0}' with version qualifier '{1}'.", Name, Version))
                {
                        Type = CommandResultType.Warning
                };
            }
            yield return VerifyWrapFile();
            yield return VeryfyWrapRepository();

            if (ShouldUpdateDescriptor)
                AddInstructionToDescriptor();

            var resolvedDependencies = PackageManager.TryResolveDependencies(DescriptorFromCommand(), Environment.RepositoriesForRead());

            if (!resolvedDependencies.IsSuccess)
            {
                yield return new GenericError("Could not find a package for dependency '{0}'.", Name);
                yield break;
            }
            var repositoriesToCopyTo = Environment.RemoteRepositories.Concat(new[]
            {
                Environment.CurrentDirectoryRepository,
                ProjectOnly ? null : Environment.SystemRepository,
                SystemOnly ? null : Environment.ProjectRepository
            });
            foreach (var msg in PackageManager.CopyPackagesToRepositories(resolvedDependencies, repositoriesToCopyTo.NotNull()))
                yield return msg;

            yield return new GenericMessage("Expanding packages to cache...");
            var packageRepositories = new[] { Environment.ProjectRepository };
            PackageManager.GetExports<IExport>("bin", Environment.ExecutionEnvironment, packageRepositories.NotNull()).ToList();
        }

        void AddInstructionToDescriptor()
        {
            // TODO: Make the environment descriptor separate from reader/writer,
            // and remove the File property on it.
            var dependLine = GetDependsLine();
            using (var fileStream = Environment.Descriptor.File.OpenAppend())
            using (var textWriter = new StreamWriter(fileStream, Encoding.UTF8))
                textWriter.WriteLine("\r\n" + dependLine);
            new DependsParser().Parse(dependLine, Environment.Descriptor);
        }

        WrapDescriptor DescriptorFromCommand()
        {
            return new WrapDescriptor
            {
                Dependencies =
                    {
                        new WrapDependency
                        {
                            Name = Name,
                            VersionVertices = Version != null
                                                  ? DependsParser.ParseVersions(Version.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)).ToList()
                                                  : new List<VersionVertice>()
                        }
                    }
            };
        }

        string GetDependsLine()
        {
            return "depends: " + Name + " " + (Version ?? string.Empty);
        }

        ICommandOutput VerifyWrapFile()
        {
            if (NoDescriptorUpdate)
                new GenericMessage("Wrap descriptor ignored.");
            return Environment.Descriptor != null
                       ? new GenericMessage(@"No wrap descriptor found, installing locally.")
                       : new GenericMessage("Wrap descriptor found.");
        }

        ICommandOutput VeryfyWrapRepository()
        {
            return Environment.ProjectRepository != null
                       ? new GenericMessage("Project repository found.")
                       : new GenericMessage("Project repository not found, installing to system repository.");
        }
    }
}