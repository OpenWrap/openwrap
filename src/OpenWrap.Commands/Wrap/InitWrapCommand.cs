using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;

namespace OpenWrap.Commands.Wrap
{
    [Command(Noun = "wrap", Verb = "init")]
    public class InitWrapCommand : AbstractCommand
    {
        const string MSBUILD_NS = "http://schemas.microsoft.com/developer/msbuild/2003";
        const string OPENWRAP_BUILD = @"..\..\wraps\openwrap\build\OpenWrap.CSharp.targets";
        bool? _allProjects;
        IEnumerable<IFile> _projectsToPatch;

        public InitWrapCommand()
        {
            Target = ".";
        }

        [CommandInput]
        public bool All
        {
            get { return _allProjects ?? false; }
            set { _allProjects = value; }
        }

        [CommandInput]
        public bool Meta { get; set; }

        [CommandInput]
        public string Projects { get; set; }

        [CommandInput(Position = 0)]
        public string Target { get; set; }

        IEnvironment Environment
        {
            get { return Services.Services.GetService<IEnvironment>(); }
        }

        IFileSystem FileSystem
        {
            get { return Services.Services.GetService<IFileSystem>(); }
        }

        public override IEnumerable<ICommandOutput> Execute()
        {
            return VerifyArguments()
                    .Concat(SetupDirectoriesAndDescriptor())
                    .Concat(ModifyProjects());
        }

        public IEnumerable<ICommandOutput> VerifyArguments()
        {
            if (!All && string.IsNullOrEmpty(Projects))
                yield return new Warning("Specify either the path to the projects you want updated or -all to find all projects automatically.");
            if (All && !string.IsNullOrEmpty(Projects))
            {
                yield return new Warning(@"No project was specified. Either specify -all for all the projects in any folders under the current path, or -Project path\to\project.csproj.");
            }
            _projectsToPatch = All ? GetAllProjects() : GetSpecificProjects();
            foreach (IFile proj in _projectsToPatch.Where(x => x.Exists == false))
                yield return new Warning("The project at path '{0}' does not exist. Check the path and try again.", proj.Path.FullPath);
        }

        static void CreateStructure(IFileSystemItem[] fileSystemItems)
        {
            foreach (IDirectory fs in fileSystemItems.OfType<IDirectory>())
                fs.MustExist();
            foreach (IFile fs in fileSystemItems.OfType<IFile>())
                fs.MustExist();
        }

        void AddOpenWrapDependency(PackageDescriptor packageDescriptor)
        {
            packageDescriptor.Dependencies.Add(new PackageDependency { Name = "openwrap", ContentOnly = true });
        }

        void AddPackageFolders(IDirectory projectDirectory, List<IFileSystemItem> items)
        {
            items.AddRange(new IFileSystemItem[]
            {
                    projectDirectory.GetDirectory("src"),
                    projectDirectory.GetDirectory("wraps")
            });
        }

        IEnumerable<ICommandOutput> CopyOpenWrap(IDirectory projectDirectory)
        {
            var packageManager = Services.Services.GetService<IPackageManager>();
            var initialDescriptor = new PackageDescriptor
            {
                    Name = "openwrap",
                    Dependencies =
                            {
                                    new PackageDependency
                                    {
                                            Name = "OpenWrap",
                                            VersionVertices = { new AnyVersionVertex() }
                                    }
                            }
            };

            DependencyResolutionResult openwrapPackage = packageManager.TryResolveDependencies(initialDescriptor, new[] { Environment.SystemRepository });
            var folderRepository = new FolderRepository(projectDirectory.GetDirectory("wraps")) { EnableAnchoring = true };
            foreach (ICommandOutput msg in packageManager.CopyPackagesToRepositories(
                    openwrapPackage,
                    Environment.SystemRepository,
                    folderRepository))
                yield return msg;
            folderRepository.Refresh();
            folderRepository.VerifyAnchors(new[] { folderRepository.PackagesByName["openwrap"].First() });
        }

        IEnumerable<IFile> GetAllProjects()
        {
            return Environment.CurrentDirectory.Files("*.csproj", SearchScope.SubFolders);
        }

        IEnumerable<IFile> GetSpecificProjects()
        {
            return string.IsNullOrEmpty(Projects)
                           ? Enumerable.Empty<IFile>()
                           : Projects.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
                                     .Select(x => Environment.CurrentDirectory.GetFile(x));
        }

        IEnumerable<ICommandOutput> ModifyProjects()
        {
            foreach (IFile project in _projectsToPatch)
            {
                var xmlDoc = new XmlDocument();
                var namespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
                namespaceManager.AddNamespace("msbuild", MSBUILD_NS);

                using (Stream projectFileStream = project.OpenRead())
                    xmlDoc.Load(projectFileStream);
                XmlNode importNode = xmlDoc.SelectSingleNode(@"//msbuild:Import[@Project='$(MSBuildToolsPath)\Microsoft.CSharp.targets']", namespaceManager);
                if (importNode == null)
                    yield return new GenericMessage("Project '{0}' was not a recognized csharp project file. Ignoring.", project.Name);
                else
                {
                    // TODO: Detect path of openwrap directory and generate correct relative path from there
                    importNode.Attributes["Project"].Value = OPENWRAP_BUILD;
                    using (Stream projectFileStream = project.OpenWrite())
                        xmlDoc.Save(projectFileStream);
                    yield return new GenericMessage(string.Format("Project '{0}' updated to use OpenWrap.", project.Path.FullPath));
                }
            }
        }

        IEnumerable<ICommandOutput> SetupDirectoriesAndDescriptor()
        {
            IDirectory currentDirectory = Environment.CurrentDirectory;

            IDirectory projectDirectory = Target == "." ? currentDirectory : currentDirectory.GetDirectory(Target);

            string packageName = Target == "." ? Environment.CurrentDirectory.Name : Target;

            IFile packageDescriptorFile = projectDirectory.GetFile(packageName + ".wrapdesc");
            if (packageDescriptorFile.Exists)
            {
                yield return new GenericMessage("Package descriptor found.");
                yield break;
            }
            var items = new List<IFileSystemItem> { packageDescriptorFile };

            var packageDescriptor = new PackageDescriptor();
            if (Meta)
            {
                packageDescriptor.BuildCommand = "$meta";
            }
            else
            {
                AddOpenWrapDependency(packageDescriptor);
                AddPackageFolders(projectDirectory, items);

                foreach (ICommandOutput m in CopyOpenWrap(projectDirectory)) yield return m;
            }
            WriteVersionFile(projectDirectory);
            WriteDescriptor(packageDescriptorFile, packageDescriptor);
            yield return new GenericMessage("Package '{0}' initialized. Start adding packages by using the 'add-wrap' command.", packageName);
        }

        void WriteDescriptor(IFile descriptor, PackageDescriptor packageDescriptor)
        {
            using (Stream descriptorStream = descriptor.OpenWrite())
            {
                new PackageDescriptorReaderWriter().Write(packageDescriptor, descriptorStream);
            }
        }

        void WriteVersionFile(IDirectory projectDirectory)
        {
            using (Stream versionFile = projectDirectory.GetFile("version").OpenWrite())
            {
                versionFile.Write(Encoding.Default.GetBytes(("0.0.1.*")));
            }
        }
    }
}