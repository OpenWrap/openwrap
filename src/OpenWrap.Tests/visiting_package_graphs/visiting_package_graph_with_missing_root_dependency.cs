using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.visiting_package_graphs
{
    class visiting_package_graph_with_missing_root_dependency : contexts.package_graph_visitor
    {
        public visiting_package_graph_with_missing_root_dependency()
        {
            given_root("one-ring");

            when_visiting_graph_from_leafs((from, dep, to) => true);
        }

        [Test]
        public void visit_succeeds()
        {
            Result.ShouldBeTrue();
        }
    }
}