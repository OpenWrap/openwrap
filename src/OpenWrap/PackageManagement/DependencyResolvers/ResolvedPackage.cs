using System.Collections.Generic;
using System.Linq;
using OpenWrap.PackageModel;

namespace OpenWrap.PackageManagement.DependencyResolvers
{
    public class ResolvedPackage
    {
        public ResolvedPackage(PackageIdentifier packageIdentifier, IEnumerable<IPackageInfo> packages, IEnumerable<CallStack> dependencies)
        {
            Identifier = packageIdentifier;
            Packages = packages.ToList().AsReadOnly();
            DependencyStacks = dependencies.ToList().AsReadOnly();
        }

        public IEnumerable<CallStack> DependencyStacks { get; private set; }
        public PackageIdentifier Identifier { get; private set; }

        public bool IsAnchored
        {
            get
            {
                return DependencyStacks.Any(stack => stack.OfType<DependencyNode>().Where(x => x.Dependency.Name.EqualsNoCase(Identifier.Name)).Any(x => x.Dependency.Anchored))
                       || Packages.Any(x => x.Anchored);
            }
        }

        public bool IsInContentBranch
        {
            get { return DependencyStacks.All(stack => stack.OfType<DependencyNode>().Any(dependency => dependency.Dependency.ContentOnly)); }
        }

        public IEnumerable<IPackageInfo> Packages { get; private set; }
    }
}