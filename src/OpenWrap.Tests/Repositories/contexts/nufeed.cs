using System;
using System.Linq;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenRasta.Client;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;
using OpenWrap.Repositories.NuFeed;
using Tests.contexts;

namespace Tests.Repositories.contexts
{
    public abstract class nufeed : http
    {
        protected ILookup<string, IPackageInfo> Packages;
        protected InMemoryFileSystem FileSystem = new InMemoryFileSystem();
        ITemporaryDirectory CacheDirectory;
        NuFeedRepository repository;


        public nufeed()
        {
            CacheDirectory = FileSystem.CreateTempDirectory();
        }
        protected void given_repository(string nugetFeedUri, bool cachingEnabled = false)
        {
            repository = new NuFeedRepository(FileSystem,
                cachingEnabled?new PackageCacheManager(CacheDirectory) : null, 
                                                        Client, nugetFeedUri.ToUri(), nugetFeedUri.ToUri());
            
        }
        protected virtual void when_reading_packages()
        {   
            Packages = repository.PackagesByName;
        }

        protected void given_packages_read_once()
        {
            Packages = repository.PackagesByName;

        }

        protected void when_updating_cache()
        {
            repository.Update();
            Packages = repository.PackagesByName;
        }
    }
}