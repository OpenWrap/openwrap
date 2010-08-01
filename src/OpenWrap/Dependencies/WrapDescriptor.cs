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
            Dependencies = new List<WrapDependency>();
            Overrides = new List<WrapOverride>();
            Description = "";
        }

        public ICollection<WrapDependency> Dependencies { get; set; }
        public ICollection<WrapOverride> Overrides { get; set; }

        public string Name { get; set; }

        public Version Version { get; set; }
        public bool IsVersionInDescriptor { get; set; }
        public IFile File { get; set; }
        public string Description { get; set; }

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

        public DateTime? LastModifiedTimeUtc
        {
            get { return null; }
        }

        public bool IsAnchored { get; set; }

        public bool IsCompatibleWith(Version version)
        {
            return false;
        }

    }
}