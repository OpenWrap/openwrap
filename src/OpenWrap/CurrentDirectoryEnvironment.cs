using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenWrap.Commands.Wrap;
using OpenWrap.Exports;
using OpenWrap.Repositories;
using IOExtensions=OpenWrap.Commands.Wrap.IOExtensions;

namespace OpenWrap
{
    public class CurrentDirectoryEnvironment : IEnvironment
    {
        public IPackageRepository ProjectRepository { get; set; }
        public string DescriptorPath { get; set; }
        public IEnumerable<IPackageRepository> RemoteRepositories { get; set; }
        public IPackageRepository UserRepository { get; set; }

        public void Initialize()
        {
            DescriptorPath = new DirectoryInfo(Environment.CurrentDirectory)
                .SelfAndAncestors()
                .SelectMany(x => x.Files("*.wrapdesc"))
                .FirstOrDefault();

            var dir = new DirectoryInfo(Path.GetDirectoryName(DescriptorPath))
                .SelfAndAncestors()
                .SelectMany(x => x.Directories("wraps"))
                .Where(x => x != null)
                .FirstOrDefault();
            if (dir != null)
                ProjectRepository = new FolderRepository(dir.FullName);

            UserRepository = OpenWrap.Repositories.UserRepository.Current;

            RemoteRepositories = UserSettings.RemoteRepositories.Select(x => new XmlRepository(new HttpNavigator(x), Enumerable.Empty<IExportBuilder>())).Cast<IPackageRepository>();
        }
    }
    public static class UserSettings
    {
        static IEnumerable<Uri> _remoteRepositoriesPaths = new[]{ new Uri("http://wraps.openrasta.com",UriKind.Absolute) };


        public static string UserRepositoryPath { get { return FileSystem.CombinePaths(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "openwrap", "wraps"); } }

        public static IEnumerable<Uri> RemoteRepositories
        {
            get { return _remoteRepositoriesPaths; }
            set { _remoteRepositoriesPaths = value; }
        }
    }
}