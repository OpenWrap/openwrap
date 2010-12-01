using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenWrap.Dependencies;
using OpenWrap.Exports;

namespace OpenWrap.Repositories
{
    public class InMemoryPackage : IPackageInfo, IPackage
    {
        public PackageIdentifier Identifier
        {
            get { return new PackageIdentifier(Name, Version); }
        }

        public ICollection<PackageDependency> Dependencies { get; set; }

        public InMemoryPackage()
        {
            CreationTime = DateTime.Now;
            Dependencies = new List<PackageDependency>();
        }
        public string FullName
        {
            get { return Name + "-" + Version; }
        }

        public DateTimeOffset CreationTime
        {
            get; private set;
        }

        public bool Anchored { get; set; }
        
        public string Name { get; set; }
        public string Description { get; set; }
        public IPackageRepository Source { get; set; }
        public Version Version { get; set; }

        public bool Nuked { get; set; }


        public IExport GetExport(string exportName, ExecutionEnvironment environment)
        {
            return null;
        }

        public Stream OpenStream()
        {
            var package = new InMemoryFile(@"c:\test.wrap");
            PackageBuilder.NewWithDescriptor(package, Name, Version.ToString(), Dependencies.Select(x=>"depends: " + x).ToArray());
            return package.OpenRead();
        }

        public IPackage Load()
        {
            return this;
        }

    }
}