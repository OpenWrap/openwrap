using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenRasta.Wrap.Tests.Dependencies.context;
using OpenFileSystem.IO;
using OpenWrap.PackageModel;
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

        public InMemoryEnvironment(IDirectory currentDirectory, IDirectory configDirectory)
        {
            CurrentDirectory = currentDirectory;
            SystemRepository = new InMemoryRepository("System repository");
            RemoteRepository = new InMemoryRepository("Remote repository");
            CurrentDirectoryRepository = new InMemoryRepository("Current directory repository"); 
            RemoteRepositories = new List<InMemoryRepository> { RemoteRepository };
            DescriptorFile = CurrentDirectory.GetFile("descriptor.wrapdesc");
            Descriptor = new PackageDescriptor();
            ConfigurationDirectory = configDirectory;
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

        public IDictionary<string, IPackageDescriptor> ScopedDescriptors
        {
            get { return null; }
        }

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