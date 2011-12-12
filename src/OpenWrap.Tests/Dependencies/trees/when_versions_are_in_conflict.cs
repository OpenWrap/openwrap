using System.Linq;
using NUnit.Framework;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Dependencies.trees
{
    public class when_versions_are_in_conflict : dependency_resolver_context
    {
        public when_versions_are_in_conflict()
        {
            given_project_package("sauron-1.0.0");
            given_project_package("sauron-1.1.0");
            given_project_package("rings-of-power-1.0.0", "depends: sauron = 1.0.0");
            given_project_package("one-ring-to-rule-them-all-1.0.0", "depends: sauron = 1.1.0");
            given_project_package("tolkien-1.0.0", "depends: rings-of-power", "depends: one-ring-to-rule-them-all");

            given_dependency("depends: tolkien");

            when_resolving_packages();
        }

        [Test]
        public void the_resolving_fails()
        {
            Resolve.IsSuccess.ShouldBeFalse();
        }
        [Test]
        public void conflicting_packages_are_present()
        {
            Resolve.DiscardedPackages.ShouldHaveCountOf(1);
            Resolve.DiscardedPackages.First()
                .Check(_ => _.Identifier.Name.ShouldBe("sauron"))
                .Check(_ => _.DependencyStacks.ShouldHaveCountOf(2));
        }
    }
}