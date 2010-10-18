using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using OpenFileSystem.IO.FileSystems;
using OpenWrap.Dependencies;
using OpenFileSystem.IO;
using OpenWrap.Repositories;
using OpenWrap.Services;

namespace OpenWrap.Commands.Wrap
{
    [Command(Noun="wrap", Verb="new")]
    public class InitWrapCommand : AbstractCommand
    {
        public InitWrapCommand()
        {
            Target = ".";
        }
        [CommandInput(Position=0)]
        public string Target { get; set; }

        IFileSystem FileSystem { get { return Services.Services.GetService<IFileSystem>(); } }
        IEnvironment Environment { get { return Services.Services.GetService<IEnvironment>(); }}

        IEnumerable<IFile> _projectsToPatch;
        const string MSBUILD_NS = "http://schemas.microsoft.com/developer/msbuild/2003";
        const string OPENWRAP_BUILD = @"..\..\wraps\openwrap\build\OpenWrap.CSharp.targets";
        bool? _allProjects;

        [CommandInput]
        public bool All
        {
            get { return _allProjects ?? false; }
            set { _allProjects = value; }
        }

        [CommandInput]
        public string Projects { get; set; }
        [CommandInput]
        public bool Meta { get; set; }

        public override IEnumerable<ICommandOutput> Execute()
        {
            return VerifyArguments()
                    .Concat(SetupDirectoriesAndDescriptor())
                    .Concat(ModifyProjects());
        }

        IEnumerable<ICommandOutput> SetupDirectoriesAndDescriptor()
        {
            var currentDirectory = Environment.CurrentDirectory;

            var projectDirectory = Target == "." ? currentDirectory : currentDirectory.GetDirectory(Target);

            var packageName = Target == "." ? Environment.CurrentDirectory.Name : Target;

            var packageDescriptorFile = projectDirectory.GetFile(packageName + ".wrapdesc");
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

                foreach(var m in CopyOpenWrap(projectDirectory)) yield return m;
            }
            WriteVersionFile(projectDirectory);
            WriteDescriptor(packageDescriptorFile, packageDescriptor);
            yield return new GenericMessage("Package '{0}' initialized. Start adding packages by using the 'add-wrap' command.", packageName);
        }

        IEnumerable<ICommandOutput> ModifyProjects()
        {

            foreach (var project in _projectsToPatch)
            {
                var xmlDoc = new XmlDocument();
                var namespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
                namespaceManager.AddNamespace("msbuild", MSBUILD_NS);

                using (var projectFileStream = project.OpenRead())
                    xmlDoc.Load(projectFileStream);
                var importNode = xmlDoc.SelectSingleNode(@"//msbuild:Import[@Project='$(MSBuildToolsPath)\Microsoft.CSharp.targets']", namespaceManager);
                if (importNode == null)
                    yield return new GenericMessage("Project '{0}' was not a recognized csharp project file. Ignoring.", project.Name);
                else
                {
                    // TODO: Detect path of openwrap directory and generate correct relative path from there
                    importNode.Attributes["Project"].Value = OPENWRAP_BUILD;
                    using (var projectFileStream = project.OpenWrite())
                        xmlDoc.Save(projectFileStream);
                    yield return new GenericMessage(string.Format("Project '{0}' updated to use OpenWrap.", project.Path.FullPath));
                }
            }
        }

        void AddOpenWrapDependency(PackageDescriptor packageDescriptor)
        {
            packageDescriptor.Dependencies.Add(new PackageDependency { Name = "openwrap", ContentOnly = true });
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

            var openwrapPackage = packageManager.TryResolveDependencies(initialDescriptor, new[] { Environment.SystemRepository });
            var folderRepository = new FolderRepository(projectDirectory.GetDirectory("wraps")) { EnableAnchoring=true};
            foreach(var msg in packageManager.CopyPackagesToRepositories(
                openwrapPackage, 
                Environment.SystemRepository,
                folderRepository))
                yield return msg;
            folderRepository.Refresh();
            folderRepository.VerifyAnchors(new[] { folderRepository.PackagesByName["openwrap"].First() });
        }

        void AddPackageFolders(IDirectory projectDirectory, List<IFileSystemItem> items)
        {
            items.AddRange(new IFileSystemItem[]
            {
                    projectDirectory.GetDirectory("src"),
                    projectDirectory.GetDirectory("wraps")
            });
        }

        void WriteDescriptor(IFile descriptor, PackageDescriptor packageDescriptor)
        {
            using (var descriptorStream = descriptor.OpenWrite())
            {
                new PackageDescriptorReaderWriter().Write(packageDescriptor, descriptorStream);
            }
        }

        void WriteVersionFile(IDirectory projectDirectory)
        {
            using(var versionFile = projectDirectory.GetFile("version").OpenWrite())
            {
                versionFile.Write(Encoding.Default.GetBytes(("0.0.1.*")));
            }
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

        public IEnumerable<ICommandOutput> VerifyArguments()
        {
            if (!All && string.IsNullOrEmpty(Projects))
                yield return new Warning("Specify either the path to the projects you want updated or -all to find all projects automatically.");
            if (All && !string.IsNullOrEmpty(Projects))
            {
                yield return new Warning(@"No project was specified. Either specify -all for all the projects in any folders under the current path, or -Project path\to\project.csproj.");
            }
            _projectsToPatch = All ? GetAllProjects() : GetSpecificProjects();
            foreach (var proj in _projectsToPatch.Where(x => x.Exists == false))
                yield return new Warning("The project at path '{0}' does not exist. Check the path and try again.", proj.Path.FullPath);
        }

        static void CreateStructure(IFileSystemItem[] fileSystemItems)
        {
            foreach (var fs in fileSystemItems.OfType<IDirectory>())
                fs.MustExist();
            foreach (var fs in fileSystemItems.OfType<IFile>())
                fs.MustExist();
        }
    }
}