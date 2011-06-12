using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;
using OpenWrap.PackageModel;
using OpenWrap.Testing;

namespace Tests.visiting_package_graphs
{
    class visiting_from_node : contexts.package_graph_visitor
    {
        //List<IPackageInfo> visited = new List<IPackageInfo>();

        public visiting_from_node()
        {
            given_package("the-shire", "1.0");
            given_package("sam", "1.0", "the-shire");
            given_package("frodo", "1.0", "the-shire");
            given_package("lotr", "1.0", "the-shire", "sam", "frodo");

            when_visiting_graph_from_leafs((from, dep, to) =>
            {
                visited.Add(to);
                return true;
            }, new PackageDependencyBuilder("frodo").Build());
        }

        [Test]
        public void visits_each_node_once()
        {
            visited.Count(x => x.Name == "the-shire").ShouldBe(1);
            visited.Count(x => x.Name == "frodo").ShouldBe(1);
            visited.Count().ShouldBe(2);
        }
    }
}