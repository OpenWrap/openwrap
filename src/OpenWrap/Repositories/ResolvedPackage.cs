using System;
using System.Collections.Generic;

namespace OpenWrap.Repositories
{
    public class ResolvedPackage
    {
        public ResolvedPackage(string packageName, IPackageInfo package, IEnumerable<ParentedDependency> dependencies)
        {
            PackageName = packageName;
            Package = package;
            Dependencies = dependencies;
        }

        public string PackageName { get; set; }
        public IPackageInfo Package { get; set; }
        public IEnumerable<ParentedDependency> Dependencies { get; set; }
        
    }
}
