using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenRasta.Wrap.Tests.Dependencies.context;
using OpenWrap.Build.Services;
using OpenWrap.Dependencies;
using OpenWrap.IO;
using OpenWrap.Repositories;

namespace OpenWrap.Tests.Commands
{
    public class InMemoryEnvironment : IEnvironment
    {
        public InMemoryRepository ProjectRepository;
        public IList<InMemoryRepository> RemoteRepositories;
        public InMemoryRepository SystemRepository;
        public InMemoryRepository RemoteRepository;
        public InMemoryRepository CurrentDirectoryRepository;

        public InMemoryEnvironment(IDirectory currentDirectory)
        {
            CurrentDirectory = currentDirectory;
            SystemRepository = new InMemoryRepository("System repository");
            RemoteRepository = new InMemoryRepository("Remote repository");
            CurrentDirectoryRepository = new InMemoryRepository("Current directory repository"); 
            RemoteRepositories = new List<InMemoryRepository> { RemoteRepository };
            Descriptor = new WrapDescriptor() { File = CurrentDirectory.GetFile("descriptor.wrapdesc").EnsureExists()};
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

        public WrapDescriptor Descriptor { get; set; }

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
    }
}