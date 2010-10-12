using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Dependencies;
using OpenFileSystem.IO;
using OpenWrap.Repositories;using OpenWrap.Services;

namespace OpenWrap.Commands.Wrap
{
    [Command(Noun="wrap", Verb="new")]
    public class NewWrapCommand : ICommand
    {
        [CommandInput(Position=0, IsRequired=true)]
        public string Target { get; set; }

        IFileSystem FileSystem { get { return Services.Services.GetService<IFileSystem>(); } }
        IEnvironment Environment { get { return Services.Services.GetService<IEnvironment>(); }}

        [CommandInput]
        public bool Meta { get; set; }

        public IEnumerable<ICommandOutput> Execute()
        {
            var currentDirectory = Environment.CurrentDirectory;
            var packagePath = Target;
            var projectDirectory = currentDirectory.GetDirectory(packagePath);
            var packageName = Target == "." ? Environment.CurrentDirectory.Name : Target;

            var packageDescriptorFile = projectDirectory.GetFile(packageName + ".wrapdesc");

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

        void AddOpenWrapDependency(PackageDescriptor packageDescriptor)
        {
            packageDescriptor.Dependencies.Add(new PackageDependency { Name = "openwrap", ContentOnly = true });
        }

        IEnumerable<ICommandOutput> CopyOpenWrap(IDirectory projectDirectory)
        {
            var packageManager = Services.Services.GetService<IPackageManager>();
            var openwrapPackage = packageManager.TryResolveDependencies(new PackageDescriptor { Name = "openwrap" }, new[] { Environment.SystemRepository });
            foreach(var msg in packageManager.CopyPackagesToRepositories(openwrapPackage, new FolderRepository(projectDirectory.GetDirectory("wraps"))))
                yield return msg;
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

        static void CreateStructure(IFileSystemItem[] fileSystemItems)
        {
            foreach (var fs in fileSystemItems.OfType<IDirectory>())
                fs.MustExist();
            foreach (var fs in fileSystemItems.OfType<IFile>())
                fs.MustExist();
        }
    }
}