using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenRasta.Wrap.Tests.Dependencies.context;
using OpenWrap.Build.Services;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;

namespace OpenWrap.Tests.Commands
{
    public class InMemoryEnvironment : IEnvironment
    {
        public InMemoryRepository ProjectRepository;
        public IList<InMemoryRepository> RemoteRepositories;
        public InMemoryRepository UserRepository;
        public InMemoryRepository RemoteRepository;

        public InMemoryEnvironment()
        {
            UserRepository = new InMemoryRepository();
            RemoteRepository = new InMemoryRepository();
            RemoteRepositories = new List<InMemoryRepository> { RemoteRepository };
            Descriptor = new WrapDescriptor();

        }

        void IService.Initialize()
        {
        }

        IPackageRepository IEnvironment.ProjectRepository
        {
            get { return ProjectRepository; }
        }

        public WrapDescriptor Descriptor { get; set; }

        IEnumerable<IPackageRepository> IEnvironment.RemoteRepositories
        {
            get { return RemoteRepositories.Cast<IPackageRepository>(); }
             
        }

        IPackageRepository IEnvironment.UserRepository
        {
            get { return UserRepository; }
        }

        public DirectoryInfo CurrentDirectory
        {
            get; set;
        }

        public ExecutionEnvironment ExecutionEnvironment
        {
            get; set;
        }
    }
}