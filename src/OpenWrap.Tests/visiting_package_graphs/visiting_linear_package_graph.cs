using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.PackageManagement.DependencyResolvers;
using OpenWrap.PackageManagement.Packages;
using OpenWrap.PackageModel;
using OpenWrap.Testing;

namespace Tests.visiting_package_graphs
{
    class visiting_linear_package_graph : contexts.package_graph_visitor
    {
        //List<IPackageInfo> visited = new List<IPackageInfo>();


        public visiting_linear_package_graph()
        {
            given_package("one-ring", "1.0");
            given_package("sauron", "1.0", "one-ring");
            when_visiting_graph_from_leafs((from, dep, to) =>
            {
                visited.Add(to);
                return true;
            });
        }

        [Test]
        public void package_with_no_dependency_called_last()
        {
            visited.Last().Name.ShouldBe("one-ring");
        }

        [Test]
        public void leaf_package_called_first()
        {
            visited.First().Name.ShouldBe("sauron");
        }
    }
}
   
