using System;
using System.Collections.Generic;
using OpenRasta.Wrap.Sources;

namespace OpenRasta.Wrap.Dependencies
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

        public bool IsCompatibleWith(Version version)
        {
            return false;
        }
    }
}