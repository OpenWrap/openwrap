using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Dependencies;

namespace OpenWrap.Repositories
{
    public class ResolvedPackage
    {
        public ResolvedPackage(PackageIdentifier packageIdentifier, IEnumerable<IPackageInfo> packages, IEnumerable<CallStack> dependencies)
        {
            Identifier = packageIdentifier;
            Packages = packages.ToList().AsReadOnly();
            DependencyStacks = dependencies.ToList().AsReadOnly();
        }

        public PackageIdentifier Identifier { get; private set; }
        public IEnumerable<IPackageInfo> Packages { get; private set; }
        public IEnumerable<CallStack> DependencyStacks { get; private set; }

        public bool IsInContentBranch
        {
            get { return DependencyStacks.All(stack => stack.OfType<DependencyNode>().Any(dependency => dependency.Dependency.ContentOnly)); }
        }
        public bool IsAnchored
        {
            get
            {
                return DependencyStacks.Any(stack => stack.OfType<DependencyNode>().Where(x => x.Dependency.Name.EqualsNoCase(Identifier.Name)).Any(x => x.Dependency.Anchored))
                       || Packages.Any(x => x.Anchored);
            }
        }
    }
}
