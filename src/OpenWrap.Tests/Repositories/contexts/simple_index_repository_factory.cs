using OpenFileSystem.IO.FileSystems.InMemory;
using OpenWrap.Repositories;
using Tests.contexts;

namespace Tests.Repositories.contexts
{
    public class simple_index_repository_factory : repository_factory<SimpleIndexedRepositoryFactory>
    {
        public simple_index_repository_factory()
            : base(_ => new SimpleIndexedRepositoryFactory(_))
        {
        }
    }
    public class nuget_repository_factory : repository_factory<NuGetODataRepositoryFactory>
    {
        public nuget_repository_factory()
            : base(_ => new NuGetODataRepositoryFactory(new InMemoryFileSystem(),_))
        {
        }
    }
}