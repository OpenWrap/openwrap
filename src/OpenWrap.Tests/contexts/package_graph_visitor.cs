using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.DependencyResolvers;
using OpenWrap.PackageManagement.Packages;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Parsers;
using OpenWrap.Testing;

namespace Tests.contexts
{
    abstract class package_graph_visitor : context
    {
        protected HashSet<IPackageInfo> Packages = new HashSet<IPackageInfo>();
        protected List<PackageDependency> Roots = new List<PackageDependency>();
        protected bool Result;
        protected List<IPackageInfo> visited = new List<IPackageInfo>();

        public void given_package(string name, string version, params string[] dependencies)
        {
            InMemoryPackage package;
            Packages.Add(package = new InMemoryPackage { Name = name, Version = version.ToVersion(), Dependencies = dependencies.Select(DependsParser.ParseDependsValue).ToList() });

        }

        public void given_root(string name)
        {
            Roots.Add(new PackageDependency(name));
        }
        protected void when_visiting_graph_from_leafs(PackageGraphVisitor.PackageVisitor visitor, params PackageDependency[] root)
        {
            Result = new PackageGraphVisitor(Packages).VisitFrom(root != null && root.Length > 0 ? root : (Roots.Count > 0 ? Roots.ToArray() : null), visitor);
        }

        protected bool VisitNodes(IPackageInfo from, PackageDependency dependency, IPackageInfo to)
        {
            visited.Add(to);
            return true;
        }
    }
}
