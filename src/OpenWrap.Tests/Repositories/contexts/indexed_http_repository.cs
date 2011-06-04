using System;
using OpenWrap.IO;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenWrap.Repositories;
using OpenWrap.Repositories.Http;
using Tests.contexts;

namespace Tests.Repositories.contexts
{
    public class indexed_http_repository : repository_factory<IndexedHttpRepositoryFactory>
    {
        public indexed_http_repository()
            : base(_ => new IndexedHttpRepositoryFactory(_))
        {
        }

    }
}