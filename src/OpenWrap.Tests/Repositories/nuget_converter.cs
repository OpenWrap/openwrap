using System;
using System.IO;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenWrap.PackageManagement.Packages;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;
using OpenWrap.Repositories.NuGet;
using OpenWrap.Tests;

namespace Tests.Repositories
{
    namespace contexts
    {
        public abstract class nuget_converter : OpenWrap.Testing.context
        {
            IFileSystem FileSystem;
            InMemoryRepository Repository = new InMemoryRepository("In memory");

            public nuget_converter()
            {
                this.FileSystem = new InMemoryFileSystem();
                //this.FileSystem = LocalFileSystem.Instance;
            }
            protected void when_converting_package()
            {

                var cacheDir = FileSystem.GetDirectory(@"c:\cache\TestPackage-1.0.0.1234");
                var wrapFile = FileSystem.GetFile(@"c:\tmp\TestPackage-1.0.0.1234.wrap");

                //c:\tmp\TestPackage-1.0.0.1234.wrap
                using(var openWrapPackage = wrapFile.OpenWrite())
                    NuGetConverter.Convert(NuPackage, openWrapPackage);
                
                Package = new CachedZipPackage(Repository, wrapFile, cacheDir);
            }

            protected IPackageInfo Package { get; set; }
            protected MemoryStream NuPackage { get; set; }

            protected void given_nuget_package(byte[] testPackage)
            {
                NuPackage = new MemoryStream(testPackage);
            }

            protected void given_readonly_nu_package(byte[] testPackage)
            {
                NuPackage = new NonSeekableMemoryStream(testPackage);
            }
        }
    }
}
