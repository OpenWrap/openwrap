using System;
using System.Collections.Generic;
using OpenWrap.Dependencies;

namespace OpenWrap.Repositories
{
    public class ResolvedDependency
    {
        public PackageDependency Dependency { get; set; }
        public IPackageInfo Package { get; set; }
        public ResolvedDependency Parent { get; set; }
    }
}
