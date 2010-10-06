using System;
using System.Collections.Generic;
using System.IO;
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
            return new MemoryStream(0);
        }

        public IPackage Load()
        {
            return this;
        }

    }
}