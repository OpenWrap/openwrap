using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystem.InMemory;
using OpenWrap.Dependencies;
using OpenWrap.Exports;

namespace OpenWrap.Repositories
{
    public class InMemoryPackage : IPackageInfo, IPackage
    {
        public ICollection<PackageDependency> Dependencies { get; set; }

        public InMemoryPackage()
        {
            LastModifiedTimeUtc = DateTime.Now;
            Dependencies = new List<PackageDependency>();
        }
        public string FullName
        {
            get { return Name + "-" + Version; }
        }

        public DateTime? LastModifiedTimeUtc
        {
            get; private set;
        }

        public string Name { get; set; }

        public IPackageRepository Source { get; set; }
        public Version Version { get; set; }

        public IExport GetExport(string exportName, ExecutionEnvironment environment)
        {
            return null;
        }

        public Stream OpenStream()
        {
            var package = new InMemoryFile(@"c:\test.wrap");
            PackageBuilder.New(package, Name, Version.ToString(), Dependencies.Select(x=>"depends: " + x).ToArray());
            return package.OpenRead();
        }

        public IPackage Load()
        {
            return this;
        }

    }
}