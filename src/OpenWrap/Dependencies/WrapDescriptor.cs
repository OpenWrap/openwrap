using System;
using System.Collections.Generic;
using OpenWrap.Repositories;

namespace OpenWrap.Dependencies
{
    public class WrapDescriptor : IPackageInfo
    {
        public WrapDescriptor()
        {
            Dependencies = new List<WrapDependency>();
        }
        public ICollection<WrapDependency> Dependencies { get; set; }

        public string Name { get; set; }

        public Version Version { get; set; }
        public IPackage Load()
        {
            return null;
        }

        public IPackageRepository Source
        {
            get { return null; }
        }

        public bool IsCompatibleWith(Version version)
        {
            return false;
        }
    }
}