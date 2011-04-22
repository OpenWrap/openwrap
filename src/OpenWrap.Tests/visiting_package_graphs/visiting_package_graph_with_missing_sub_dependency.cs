using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.visiting_package_graphs
{
    class visiting_package_graph_with_missing_sub_dependency : contexts.package_graph_visitor
    {
        public visiting_package_graph_with_missing_sub_dependency()
        {
            given_package("one-ring", "1.0", "unknown");
            given_root("one-ring");

            when_visiting_graph_from_leafs(VisitNodes);
        }

        [Test]
        public void visit_succeeds()
        {
            Result.ShouldBeTrue();
        }

        [Test]
        public void found_packages_are_visited()
        {
            visited.ShouldHaveAtLeastOne(x => x.Name == "one-ring");
        }
    }
}