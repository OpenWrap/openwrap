using System.Linq;
using NUnit.Framework;
using OpenWrap.Testing;
using SpecExtensions = OpenWrap.Testing.SpecExtensions;

namespace Tests.visiting_package_graphs
{
    /// <summary>
    /// A -> B, B -> A, root (A|B)
    /// A -> A (root A)
    /// A -> B, B-> A, root null
    /// </summary>
    /// 
    abstract class visiting_recursive_package_graph : contexts.package_graph_visitor
    {
        class circular_dependency_without_roots : visiting_recursive_package_graph
        {
            public circular_dependency_without_roots()
            {
                given_package("evil", "1.0", "good");
                given_package("good", "1.0", "evil");
                when_visiting_graph_from_leafs(VisitNodes);
            }
        }
        class circular_dependency_with_roots : visiting_recursive_package_graph
        {
            public circular_dependency_with_roots()
            {
                given_package("evil", "1.0", "good");
                given_package("good", "1.0", "evil");
                given_root("evil");
                when_visiting_graph_from_leafs(VisitNodes);
            }
        }
        class self_referencing_without_roots : visiting_recursive_package_graph
        {
            public self_referencing_without_roots()
            {
                given_package("existentialism", "1.0", "existentialism");
                when_visiting_graph_from_leafs(VisitNodes);
            }
        }
        class self_referencing_with_root : visiting_recursive_package_graph
        {
            public self_referencing_with_root()
            {
                given_package("existentialism", "1.0", "existentialism");
                given_root("existentialism");
                when_visiting_graph_from_leafs(VisitNodes);
            }
        }
        class multiple_independent_self_referencing : visiting_recursive_package_graph
        {
            public multiple_independent_self_referencing()
            {
                given_package("existentialist", "1.0", "existentialist");
                given_package("navel-glazer", "1.0", "navel-glazer");
                when_visiting_graph_from_leafs(VisitNodes);

            }
        }

        [Test]
        public void all_pacakges_are_visited()
        {
            Packages.All(x => visited.Contains(x)).ShouldBeTrue();
        }

        [Test]
        public void visit_succeeds()
        {
            Result.ShouldBeTrue();
        }
    }
}