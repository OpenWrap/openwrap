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

        public IEnumerable<ICommandResult> Execute()
        {
            var currentDirectory = Environment.CurrentDirectory;
            var projectDirectory = currentDirectory.GetDirectory(ProjectName);

            CreateStructure(new IFileSystemItem[]
            {
                    projectDirectory.GetDirectory("src"),
                    projectDirectory.GetDirectory("wraps"),
                    projectDirectory.GetFile(ProjectName + ".wrapdesc")
            });
            yield return new GenericMessage("Created default project structure for '" + ProjectName + "'. Copying OpenWrap.");

            var packageManager = WrapServices.GetService<IPackageManager>();
            var openwrapPackage = packageManager.TryResolveDependencies(new WrapDescriptor { Name = "openwrap" }, new[] { Environment.SystemRepository });
            foreach(var msg in packageManager.CopyPackagesToRepositories(openwrapPackage, new FolderRepository(projectDirectory.GetDirectory("wraps"))))
                yield return msg;
        }

        void CreateStructure(IFileSystemItem[] fileSystemItems)
        {
            foreach (var fs in fileSystemItems.OfType<IDirectory>())
                fs.EnsureExists();
            foreach (var fs in fileSystemItems.OfType<IFile>())
                fs.EnsureExists();
        }
    }
}