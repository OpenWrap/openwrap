using OpenFileSystem.IO.FileSystems.InMemory;
using OpenWrap.Repositories;
using OpenWrap.Repositories.NuFeed;
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
    public class nuget_repository_factory : repository_factory<NuFeedRepositoryFactory>
    {
        public nuget_repository_factory()
            : base(_ => new NuFeedRepositoryFactory(new InMemoryFileSystem(),_))
        {
        }
    }
}