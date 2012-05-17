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

        public nufeed()
        {
            CacheDirectory = FileSystem.CreateTempDirectory();

        }

        protected void when_reading_packages()
        {
            Packages = new NuFeedRepository(FileSystem, 
                new PackageCacheManager(CacheDirectory),
                base.Client, "http://localhost/packages/1".ToUri(), "http://localhost/packages/1".ToUri())
                .PackagesByName;
        }
        
    }
}