using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

using OpenWrap.Dependencies;
using OpenWrap.Exports;
using OpenWrap.IO;
using OpenWrap.Repositories;

namespace OpenWrap
{
    public class CurrentDirectoryEnvironment : IEnvironment
    {
        public IPackageRepository ProjectRepository { get; set; }
        public WrapDescriptor Descriptor { get; set; }
        public IEnumerable<IPackageRepository> RemoteRepositories { get; set; }
        public IPackageRepository UserRepository { get; set; }
        public IFileSystem FileSystem { get; set; }

        public IDirectory CurrentDirectory
        {
            get { return IO.FileSystem.Local.GetDirectory(Environment.CurrentDirectory); }
        }

        public ExecutionEnvironment ExecutionEnvironment
        {
            get; private set;
        }

        public void Initialize()
        {
            FileSystem = IO.FileSystem.Local;
            Descriptor = CurrentDirectory
                .AncestorsAndSelf()
                .SelectMany(x => x.Files("*.wrapdesc"))
                .Select(x=>new WrapDescriptorParser().ParseFile(x))
                .FirstOrDefault();

            var dir = Descriptor.File.Parent
                .AncestorsAndSelf()
                .SelectMany(x => x.Directories("wraps"))
                .Where(x => x != null)
                .FirstOrDefault();

            if (dir != null)
                ProjectRepository = new FolderRepository(dir);

            UserRepository = new FolderRepository(FileSystem.GetDirectory(UserSettings.UserRepositoryPath));

            RemoteRepositories = UserSettings.RemoteRepositories
                .Select(x => new XmlRepository(FileSystem, new HttpNavigator(x), Enumerable.Empty<IExportBuilder>()))
                .Cast<IPackageRepository>()
                .ToList();

            ExecutionEnvironment = new ExecutionEnvironment
            {
                Platform = IntPtr.Size == 4 ? "x86" : "x64",
                Profile = "net35"
            };
        }
    }
}