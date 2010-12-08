using System;
using System.Collections.Generic;
using System.Linq;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.Local;
using OpenWrap.Configuration;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;
using OpenWrap.Repositories.Http;
using OpenWrap.Repositories.NuGet;
using OpenWrap.Services;

namespace OpenWrap
{
    public class CurrentDirectoryEnvironment : IEnvironment
    {
        public CurrentDirectoryEnvironment()
        {
            CurrentDirectory = LocalFileSystem.Instance.GetDirectory(Environment.CurrentDirectory);
        }

        public CurrentDirectoryEnvironment(string currentDirectory)
        {
            CurrentDirectory = LocalFileSystem.Instance.GetDirectory(currentDirectory);
        }

        public IDirectory ConfigurationDirectory { get; private set; }
        public IDirectory CurrentDirectory { get; set; }

        public IPackageRepository CurrentDirectoryRepository { get; set; }

        public IFile DescriptorFile { get; private set; }

        public PackageDescriptor Descriptor { get; set; }
        public ExecutionEnvironment ExecutionEnvironment { get; private set; }
        public IFileSystem FileSystem { get; set; }
        public IPackageRepository ProjectRepository { get; set; }
        public IEnumerable<IPackageRepository> RemoteRepositories { get; set; }
        public IPackageRepository SystemRepository { get; set; }

        public void Initialize()
        {
            FileSystem = LocalFileSystem.Instance;
            DescriptorFile = CurrentDirectory
                    .AncestorsAndSelf()
                    .SelectMany(x => x.Files("*.wrapdesc"))
                    .FirstOrDefault();
            if (DescriptorFile != null)
                Descriptor = new PackageDescriptorReaderWriter().Read(DescriptorFile);

            TryInitializeProjectRepository();

            CurrentDirectoryRepository = new CurrentDirectoryRepository();

            SystemRepository = new FolderRepository(FileSystem.GetDirectory(InstallationPaths.SystemRepositoryDirectory))
            {
                Name = "System repository"
            };

            ConfigurationDirectory = FileSystem.GetDirectory(InstallationPaths.ConfigurationDirectory);

            RemoteRepositories = Services.Services.GetService<IConfigurationManager>().LoadRemoteRepositories()
                    .OrderBy(x => x.Value.Priority)
                    .Select(x => CreateRemoteRepository(x.Key, x.Value.Href))
                    .Where(x => x != null)
                    .ToList();

            ExecutionEnvironment = new ExecutionEnvironment
            {
                Platform = IntPtr.Size == 4 ? "x86" : "x64",
                Profile = "net35"
            };
        }

        void TryInitializeProjectRepository()
        {
            if (Descriptor == null || DescriptorFile == null)
                return;
            if (Descriptor.UseProjectRepository)
            {
                var projectRepositoryDirectory = DescriptorFile.Parent.FindProjectRepositoryDirectory();


                if (projectRepositoryDirectory != null)
                    ProjectRepository = new FolderRepository(projectRepositoryDirectory)
                    {
                            Name = "Project repository",
                            EnableAnchoring=true
                    };
            }
        }

        IPackageRepository CreateRemoteRepository(string repositoryName, Uri repositoryHref)
        {
            try
            {
                if (repositoryHref.Scheme.Equals("nuget", StringComparison.OrdinalIgnoreCase)
                    || repositoryHref.Scheme.Equals("nupack", StringComparison.OrdinalIgnoreCase))
                {
                    var builder = new UriBuilder(repositoryHref);
                    builder.Scheme = "http";
                    return new HttpRepository(
                            FileSystem,
                            repositoryName,
                            new NuGetFeedNavigator(builder.Uri));
                }
                if (repositoryHref.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase) ||
                    repositoryHref.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
                    return new HttpRepository(FileSystem, repositoryName, new HttpRepositoryNavigator(repositoryHref));
                if (repositoryHref.Scheme.Equals("file", StringComparison.OrdinalIgnoreCase))
                    return new IndexedFolderRepository(repositoryName, FileSystem.GetDirectory(repositoryHref.LocalPath));
            }
            catch
            {
            }
            return null;
        }
    }
}