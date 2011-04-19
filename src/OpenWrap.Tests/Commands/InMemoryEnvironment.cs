using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenFileSystem.IO;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Serialization;
using OpenWrap.Repositories;
using OpenWrap.Runtime;
using OpenWrap.Services;

namespace OpenWrap.Tests.Commands
{
    public class InMemoryEnvironment : IEnvironment
    {
        public IPackageRepository ProjectRepository;
        public IList<InMemoryRepository> RemoteRepositories;
        public InMemoryRepository SystemRepository;
        public InMemoryRepository RemoteRepository;
        public IPackageRepository CurrentDirectoryRepository;

        public InMemoryEnvironment(IDirectory currentDirectory, IDirectory configDirectory = null)
        {
            CurrentDirectory = currentDirectory;
            SystemRepository = new InMemoryRepository("System repository");
            RemoteRepository = new InMemoryRepository("Remote repository");
            CurrentDirectoryRepository = new InMemoryRepository("Current directory repository"); 
            RemoteRepositories = new List<InMemoryRepository> { RemoteRepository };
            DescriptorFile = CurrentDirectory.GetFile("descriptor.wrapdesc").MustExist();
            Descriptor = new PackageDescriptor();
            ConfigurationDirectory = configDirectory;
            ScopedDescriptors = new Dictionary<string, FileBased<IPackageDescriptor>>(StringComparer.OrdinalIgnoreCase);
            ScopedDescriptors[string.Empty] = FileBased.New(DescriptorFile, Descriptor);
        }

        void IService.Initialize()
        {
        }

        IPackageRepository IEnvironment.ProjectRepository
        {
            get { return ProjectRepository; }
        }

        IPackageRepository IEnvironment.CurrentDirectoryRepository
        {
            get { return CurrentDirectoryRepository; }
        }

        public IFile DescriptorFile { get; set; }

        public IDictionary<string, FileBased<IPackageDescriptor>> ScopedDescriptors { get; private set; }

        public IPackageDescriptor Descriptor { get; set; }

        IEnumerable<IPackageRepository> IEnvironment.RemoteRepositories
        {
            get { return RemoteRepositories.Cast<IPackageRepository>(); }
             
        }

        IPackageRepository IEnvironment.SystemRepository
        {
            get { return SystemRepository; }
        }

        public IDirectory CurrentDirectory
        {
            get; set;
        }

        public ExecutionEnvironment ExecutionEnvironment
        {
            get; set;
        }

        public IDirectory ConfigurationDirectory { get; set; }
    }
}