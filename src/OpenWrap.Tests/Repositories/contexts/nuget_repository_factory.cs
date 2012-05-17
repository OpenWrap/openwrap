using System;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenRasta.Client;
using OpenWrap.Repositories.NuFeed;
using Tests.contexts;

namespace Tests.Repositories.contexts
{
    public abstract class nuget_repository_factory : repository_factory<NuFeedRepositoryFactory, NuFeedRepository>
    {

        public nuget_repository_factory()
            : base(CreateNuFeedRepo)
        {
        }

        static NuFeedRepositoryFactory CreateNuFeedRepo(IHttpClient client)
        {
            var inMem = new InMemoryFileSystem();
            return new NuFeedRepositoryFactory(inMem,inMem.CreateTempDirectory(), client);
        }
    }
}