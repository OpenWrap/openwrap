using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Dependencies;
using OpenFileSystem.IO;
using OpenWrap.Repositories;
using OpenWrap.Services;

namespace OpenWrap.Commands.Wrap
{
    [Command(Noun="wrap", Verb="new")]
    public class NewWrapCommand : ICommand
    {
        [CommandInput(Name="ProjectName", Position=0, IsRequired=true)]
        public string ProjectName { get; set; }

        IFileSystem FileSystem { get { return WrapServices.GetService<IFileSystem>(); } }
        IEnvironment Environment { get { return WrapServices.GetService<IEnvironment>(); }}

        [CommandInput]
        public bool Meta { get; set; }

        public IEnumerable<ICommandOutput> Execute()
        {
            var currentDirectory = Environment.CurrentDirectory;
            var projectDirectory = currentDirectory.GetDirectory(ProjectName);
            var items = new List<IFileSystemItem> {
                        projectDirectory.GetFile(ProjectName + ".wrapdesc")};
            if (!Meta)
            {
                items.Add(projectDirectory.GetDirectory("src"));
                items.Add(projectDirectory.GetDirectory("wraps"));
            }
            CreateStructure(items);

            yield return new GenericMessage("Created default project structure for '" + ProjectName + "'. Copying OpenWrap.");
            if (!Meta)
            {
                var packageManager = WrapServices.GetService<IPackageManager>();
                var openwrapPackage = packageManager.TryResolveDependencies(new WrapDescriptor { Name = "openwrap" }, new[] { Environment.SystemRepository });
                foreach(var msg in packageManager.CopyPackagesToRepositories(openwrapPackage, new FolderRepository(projectDirectory.GetDirectory("wraps"))))
                    yield return msg;
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