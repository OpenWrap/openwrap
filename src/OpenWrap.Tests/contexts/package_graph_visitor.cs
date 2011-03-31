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
        protected List<IPackageInfo> Packages;

        public package_graph_visitor()
        {
            Packages = new List<IPackageInfo>();
        }
        protected void given_package(string name, string version, params string[] dependencies)
        {
            InMemoryPackage package;
            Packages.Add(package = new InMemoryPackage { Name = name, Version = version.ToVersion(), Dependencies = dependencies.Select(DependsParser.ParseDependsValue).ToList() });

        }
        protected void when_visiting_graph_from_leafs(PackageGraphVisitor.PackageVisitor visitor, params PackageDependency[] root)
        {
            new PackageGraphVisitor(Packages).VisitFromLeafs(visitor, root != null && root.Length > 0 ? root : null);
        }
    }
}
