using System;
using System.Collections;
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

            RemoteRepositories = new RemoteRepositoryManager(FileSystem, Services.Services.GetService<IConfigurationManager>()).GetInstances().ToList();
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
                {
                    var repositoryOptions = FolderRepositoryOptions.AnchoringEnabled;
                    if (Descriptor.UseSymLinks)
                        repositoryOptions |= FolderRepositoryOptions.UseSymLinks;
                    ProjectRepository = new FolderRepository(projectRepositoryDirectory, repositoryOptions)
                    {
                            Name = "Project repository"
                    };
                }
            }
        }

    }

    public interface IRemoteRepositoryManager
    {
        IPackageRepository CreateRepositoryInstance(string repositoryName, Uri repositoryHref);
        IEnumerable<IPackageRepository> GetInstances();
    }

    public class RemoteRepositoryManager : IRemoteRepositoryManager
    {
        readonly IFileSystem _fileSystem;
        IConfigurationManager _configurationManager;

        public RemoteRepositoryManager()
            : this(Services.Services.GetService<IFileSystem>(), Services.Services.GetService<IConfigurationManager>())
        {
            
        }
        public RemoteRepositoryManager(IFileSystem fileSystem, IConfigurationManager configurationManager)
        {
            _fileSystem = fileSystem;
            _configurationManager = configurationManager;
        }

        public IPackageRepository CreateRepositoryInstance(string repositoryName, Uri repositoryHref)
        {
            try
            {
                if (repositoryHref.Scheme.EqualsNoCase("nuget")
                    || repositoryHref.Scheme.EqualsNoCase("nupack"))
                {
                    var builder = new UriBuilder(repositoryHref);
                    builder.Scheme = "http";
                    return new HttpRepository(
                            _fileSystem,
                            repositoryName,
                            new NuGetFeedNavigator(builder.Uri));
                }
                if (repositoryHref.Scheme.EqualsNoCase("http") ||
                    repositoryHref.Scheme.EqualsNoCase("https"))
                    return new HttpRepository(_fileSystem, repositoryName, new HttpRepositoryNavigator(repositoryHref));
                if (repositoryHref.Scheme.EqualsNoCase("file"))
                    return new IndexedFolderRepository(repositoryName, _fileSystem.GetDirectory(repositoryHref.LocalPath));
            }
            catch
            {
            }
            return null;
        }

        public IEnumerable<IPackageRepository> GetInstances()
        {
            return _configurationManager.LoadRemoteRepositories()
                    .OrderBy(x => x.Value.Priority)
                    .Select(x => CreateRepositoryInstance(x.Key, x.Value.Href))
                    .Where(x => x != null);
        }
    }
}