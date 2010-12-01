using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems;
using OpenWrap.Dependencies;
using OpenWrap.PackageManagement;
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

        [CommandInput]
        public bool Git { get; set; }

        [CommandInput]
        public bool Hg { get; set; }

        [CommandInput]
        public bool Bazaar { get; set; }

        [CommandInput]
        public string IgnoreFileName { get; set; }

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

            if (Git)
                IgnoreFileName = ".gitignore";
            if (Hg)
                IgnoreFileName = ".hgignore";
            if (Bazaar)
                IgnoreFileName = ".bzrignore";
        }

        void AddOpenWrapDependency(PackageDescriptor packageDescriptor)
        {
            packageDescriptor.Dependencies.Add(new PackageDependency { Name = "openwrap", ContentOnly = true, Anchored = true });
        }

        void AddPackageFolders(IDirectory projectDirectory)
        {
            projectDirectory.GetDirectory("src").MustExist();
            projectDirectory.GetDirectory("wraps").GetDirectory("_cache").MustExist();
        }

        IEnumerable<ICommandOutput> CopyOpenWrap(PackageDescriptor projectDescriptor, IDirectory projectDirectory)
        {
            var packageManager = Services.Services.GetService<IPackageManager>();

            var projectRepository = new FolderRepository(projectDirectory.GetDirectory("wraps"))
            {
                    EnableAnchoring = true,
                    Name = "Project repository"
            };
            packageManager.AddProjectPackage(PackageRequest.Any("openwrap"),
                                             new[] { Environment.SystemRepository },
                                             projectDescriptor,
                                             projectRepository,
                                             PackageAddOptions.Default | PackageAddOptions.Anchor | PackageAddOptions.Content).ToList();
            yield return new GenericMessage("Project repository initialized.");
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
                var csharpTarget = (from node in xmlDoc.SelectNodes(@"//msbuild:Import", namespaceManager).OfType<XmlElement>()
                                   let attr = node.GetAttribute("Project")
                                   where attr != null && attr.EndsWith("Microsoft.CSharp.targets")
                                   select node).FirstOrDefault();

                if (csharpTarget == null)
                    yield return new GenericMessage("Project '{0}' was not a recognized csharp project file. Ignoring.", project.Name);
                else
                {
                    // TODO: Detect path of openwrap directory and generate correct relative path from there
                    csharpTarget.Attributes["Project"].Value = OPENWRAP_BUILD;
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

            var packageDescriptor = new PackageDescriptor(){Name = packageName};
            if (Meta)
            {
                packageDescriptor.BuildCommand = "$meta";
            }
            else
            {
                AddPackageFolders(projectDirectory);
                AddIgnores(projectDirectory);
                foreach (ICommandOutput m in CopyOpenWrap(packageDescriptor, projectDirectory)) yield return m;
            }
            WriteVersionFile(projectDirectory);
            WriteDescriptor(packageDescriptorFile, packageDescriptor);
            yield return new GenericMessage("Package '{0}' initialized. Start adding packages by using the 'add-wrap' command.", packageName);
        }

        void AddIgnores(IDirectory projectDirectory)
        {
            if (IgnoreFileName == null) return;
            projectDirectory.GetDirectory("wraps").GetFile(IgnoreFileName).WriteString("_cache\r\n_cache\\*");
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
                versionFile.Write(Encoding.UTF8.GetBytes(("0.0.1.*")));
            }
        }
    }
}