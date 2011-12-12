using System;
using System.Linq;
using NUnit.Framework;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Dependencies.trees
{
    public class versions_in_conflict_and_dependency_override : dependency_resolver_context
    {
        public versions_in_conflict_and_dependency_override()
        {
            given_project_package("sauron-1.0.0");
            given_project_package("sauron-1.1.0");
            given_project_package("rings-of-power-1.0.0", "depends: sauron = 1.0.0");
            given_project_package("one-ring-to-rule-them-all-1.0.0", "depends: sauron = 1.1.0");
            given_project_package("tolkien-1.0.0", "depends: rings-of-power", "depends: one-ring-to-rule-them-all");

            given_dependency("depends: tolkien");
            given_dependency("depends: sauron = 1.0.0");

            when_resolving_packages();
        }

        [Test]
        public void local_declaration_overrides_package_dependency()
        {
            Resolve.IsSuccess.ShouldBeTrue();
            Resolve.SuccessfulPackages.Count().ShouldBe(4);
            Resolve.SuccessfulPackages.ToLookup(x => x.Identifier.Name)["sauron"]
                .ShouldHaveCountOf(1)
                .First().Identifier.Version.ShouldBe("1.0.0");
        }
    }
}