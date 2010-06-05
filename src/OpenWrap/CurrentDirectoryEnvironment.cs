using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using OpenWrap.Commands.Wrap;
using OpenWrap.Dependencies;
using OpenWrap.Exports;
using OpenWrap.Repositories;
using IOExtensions=OpenWrap.Commands.Wrap.IOExtensions;

namespace OpenWrap
{
    public class CurrentDirectoryEnvironment : IEnvironment
    {
        public IPackageRepository ProjectRepository { get; set; }
        public WrapDescriptor Descriptor { get; set; }
        public IEnumerable<IPackageRepository> RemoteRepositories { get; set; }
        public IPackageRepository UserRepository { get; set; }

        public DirectoryInfo CurrentDirectory
        {
            get { return new DirectoryInfo(Environment.CurrentDirectory); }
        }

        public ExecutionEnvironment ExecutionEnvironment
        {
            get; private set;
        }

        public void Initialize()
        {
            Descriptor = new DirectoryInfo(Environment.CurrentDirectory)
                .SelfAndAncestors()
                .SelectMany(x => x.Files("*.wrapdesc"))
                .Select(x=>new WrapDescriptorParser().ParseFile(x))
                .FirstOrDefault();

            var dir = new DirectoryInfo(Path.GetDirectoryName(Descriptor.Path))
                .SelfAndAncestors()
                .SelectMany(x => x.Directories("wraps"))
                .Where(x => x != null)
                .FirstOrDefault();
            if (dir != null)
                ProjectRepository = new FolderRepository(dir.FullName);

            UserRepository = Repositories.UserRepository.Current;

            RemoteRepositories = UserSettings.RemoteRepositories
                .Select(x => new XmlRepository(new HttpNavigator(x), Enumerable.Empty<IExportBuilder>()))
                .Cast<IPackageRepository>()
                .ToList();

            ExecutionEnvironment = new ExecutionEnvironment
            {
                Platform = IntPtr.Size == 4 ? "x86" : "x64",
                Profile = "net35"
            };
        }
    }
    public static class UserSettings
    {
        static IEnumerable<Uri> _remoteRepositoriesPaths = new[]{ new Uri("http://localhost:42",UriKind.Absolute) };


        public static string UserRepositoryPath { get { return FileSystem.CombinePaths(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "openwrap", "wraps"); } }

        public static IEnumerable<Uri> RemoteRepositories
        {
            get { return _remoteRepositoriesPaths; }
            set { _remoteRepositoriesPaths = value; }
        }
    }
}