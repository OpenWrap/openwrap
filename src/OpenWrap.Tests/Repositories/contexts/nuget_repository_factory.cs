using OpenFileSystem.IO.FileSystems.InMemory;
using OpenWrap.Repositories.NuFeed;
using Tests.contexts;

namespace Tests.Repositories.contexts
{
    public class nuget_repository_factory : repository_factory<NuFeedRepositoryFactory, NuFeedRepository>
    {
        public nuget_repository_factory()
            : base(_ => new NuFeedRepositoryFactory(new InMemoryFileSystem(),_))
        {
        }
    }
}