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
    public abstract class WrapCommand : AbstractCommand
    {
        public IEnvironment Environment
        {
            get { return Services.Services.GetService<IEnvironment>(); }

        }

        protected IPackageManager PackageManager
        {
            get { return Services.Services.GetService<IPackageManager>(); }
        }

        protected DependencyResolutionResult ResolveDependencies(PackageDescriptor packageDescriptor, IEnumerable<IPackageRepository> repos)
        {
            return PackageManager.TryResolveDependencies(packageDescriptor, repos);
        }
    }

    [Command(Verb = "add", Noun = "wrap")]
    public class AddWrapCommand : WrapCommand
    {
        [CommandInput(IsRequired = true, Position = 0)]
        public string Name { get; set; }

        [CommandInput]
        public bool NoDescriptorUpdate { get; set; }

        [CommandInput]
        public bool ProjectOnly { get; set; }

        [CommandInput]
        public bool SystemOnly { get; set; }

        [CommandInput]
        public bool Content { get; set; }

        [CommandInput(Position = 1)]
        public string Version { get; set; }

        [CommandInput]
        public bool Anchored { get; set; }


        bool ShouldUpdateDescriptor
        {
            get
            {
                return NoDescriptorUpdate == false &&
                       Environment.Descriptor != null &&
                       SystemOnly == false;
            }
        }

        public override IEnumerable<ICommandOutput> Execute()
        {
            if (Name.EndsWith(".wrap", StringComparison.OrdinalIgnoreCase))
            {
                var desc = WrapFileToPackageDescriptor();
                yield return desc;
            }

            yield return VerifyWrapFile();
            yield return VeryfyWrapRepository();

            var packageDescriptor = DescriptorFromCommand();

            if (ShouldUpdateDescriptor)
                yield return UpdateDescriptor();

            var sourceRepositories = Environment.RepositoriesForRead();
            var resolvedDependencies = ResolveDependencies(packageDescriptor, sourceRepositories);

            if (!resolvedDependencies.IsSuccess)
            {
                yield return new DependencyResolutionFailedResult(string.Format("Could not find a package for dependency '{0}'.", Name), resolvedDependencies);
                yield break;
            }

            var repositoriesToCopyTo = Environment.RemoteRepositories.Concat(new[]
            {
                Environment.CurrentDirectoryRepository,
                ProjectOnly ? null : Environment.SystemRepository,
                SystemOnly ? null : Environment.ProjectRepository
            }).NotNull();

            foreach (var msg in PackageManager.CopyPackagesToRepositories(resolvedDependencies, repositoriesToCopyTo.NotNull()))
                yield return msg;

            foreach (var m in PackageManager.VerifyPackageCache(Environment, Environment.Descriptor)) yield return m;
        }

        ICommandOutput UpdateDescriptor()
        {
            ICommandOutput outputMessage;
            var dependencyWithSameName = Environment.Descriptor.Dependencies.Where(x => x.Name.Equals(Name, StringComparison.OrdinalIgnoreCase)).ToList();
            if (dependencyWithSameName.Count > 0)
            {
                outputMessage = new GenericMessage("Dependency already declared in descriptor, updating.");
                foreach(var i in dependencyWithSameName)
                    Environment.Descriptor.Dependencies.Remove(i);
            }
            else
            {
                outputMessage = new GenericMessage("Dependency added to descriptor.");
            }
            Environment.Descriptor.Dependencies.Add(new PackageDependency
            {
                    Anchored = Anchored, 
                    Name = Name, 
                    VersionVertices = VersionVertices(),
                    ContentOnly = Content
            });
            using(var descriptor = Environment.DescriptorFile.OpenWrite())
                new PackageDescriptorReaderWriter().Write(Environment.Descriptor, descriptor);
            return outputMessage;
        }

        ICommandOutput WrapFileToPackageDescriptor()
        {
            if (Path.GetExtension(Name).Equals(".wrap", StringComparison.OrdinalIgnoreCase) && Environment.CurrentDirectory.GetFile(Path.GetFileName(Name)).Exists)
            {
                var originalName = Name;
                Name = PackageNameUtility.GetName(Path.GetFileNameWithoutExtension(Name));
                Version = "= " + PackageNameUtility.GetVersion(Path.GetFileNameWithoutExtension(originalName));
                return
                        new GenericMessage(
                                string.Format("The requested package contained '.wrap' in the name. Assuming you pointed to the file in the current directory and meant a package named '{0}' with version qualifier '{1}'.",
                                              Name,
                                              Version))
                        {
                            Type = CommandResultType.Warning
                        };
            }
            if (File.Exists(Name))
            {
                return new GenericError("You have given a path to a .wrap file that is not in the current directory but exists on disk. This is not currently supported. Go to the directory, and re-issue the command.");
            }
            return null;
        }

        PackageDescriptor DescriptorFromCommand()
        {
            return new PackageDescriptor
            {
                Dependencies =
                    {
                        new PackageDependency
                        {
                            Name = Name,
                            Anchored = Anchored,
                            ContentOnly = Content,
                            VersionVertices = Version != null
                                                  ? VersionVertices()
                                                  : new List<VersionVertex>{new AnyVersionVertex()}
                        }
                    }
            };
        }

        List<VersionVertex> VersionVertices()
        {
            if (Version == null)
                return new List<VersionVertex>();
            return DependsParser.ParseVersions(Version.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)).ToList();
        }


        ICommandOutput VerifyWrapFile()
        {
            if (NoDescriptorUpdate)
                new GenericMessage("Wrap descriptor ignored.");
            return Environment.Descriptor == null
                       ? new GenericMessage(@"No wrap descriptor found, installing locally.")
                       : new GenericMessage("Wrap descriptor found.");
        }

        ICommandOutput VeryfyWrapRepository()
        {
            return Environment.ProjectRepository != null
                       ? new GenericMessage("Project repository present.")
                       : new GenericMessage("Project repository not found.");
        }
    }
}