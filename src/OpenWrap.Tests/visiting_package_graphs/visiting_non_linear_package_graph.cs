using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenWrap.PackageModel;
using OpenWrap.Testing;

namespace Tests.visiting_package_graphs
{
    class visiting_non_linear_package_graph : contexts.package_graph_visitor
    {
        public visiting_non_linear_package_graph()
        {
            given_package("the-shire", "1.0");
            given_package("sam", "1.0", "the-shire");
            given_package("frodo", "1.0", "the-shire");
            given_package("lotr", "1.0", "the-shire", "sam", "frodo");

            when_visiting_graph_from_leafs((from, dep, to) =>
            {
                visited.Add(to);
                return true;
            });
        }


        [Test]
        public void leaf_node_called_first()
        {
            visited.First().Name.ShouldBe("lotr");

        }
        [Test]
        public void root_node_is_called_last()
        {
            visited.Last().Name.ShouldBe("the-shire");
        }

        [Test]
        public void root_node_is_called_once_per_dependent_package()
        {
            visited.Where(x => x.Name == "the-shire").ShouldHaveCountOf(3);
        }
    }
}