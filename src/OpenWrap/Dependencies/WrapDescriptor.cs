using System;
using System.Collections.Generic;
using OpenFileSystem.IO;
using OpenWrap.Repositories;

namespace OpenWrap.Dependencies
{
    public class WrapDescriptor : IPackageInfo
    {
        public WrapDescriptor()
        {
            Dependencies = new List<PackageDependency>();
            Overrides = new List<PackageNameOverride>();
            Description = "";
            UseProjectRepository = true;
        }

        public ICollection<PackageDependency> Dependencies { get; set; }
        public ICollection<PackageNameOverride> Overrides { get; set; }

        public string Name { get; set; }

        public Version Version { get; set; }
        public bool IsVersionInDescriptor { get; set; }
        public IFile File { get; set; }
        public string Description { get; set; }
        public bool UseProjectRepository { get; set; }

        public IPackage Load()
        {
            return null;
        }

        public IPackageRepository Source
        {
            get { return null; }
        }

        public string FullName
        {
            get { return Name + "-" + Version; }
        }

        public DateTimeOffset CreationTime { get { return DateTimeOffset.UtcNow; } }

        public bool Anchored { get; set; }

        public string BuildCommand { get; set; }

        public bool IsCompatibleWith(Version version)
        {
            return false;
        }
    }
}