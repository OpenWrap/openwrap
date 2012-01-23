using System.Linq;
using NUnit.Framework;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Dependencies.trees
{
    public class dependency_leveling_across_dependencies : dependency_resolver_context
    {
        public dependency_leveling_across_dependencies()
        {
            given_project_package("rings-of-power-1.0.0");
            given_project_package("rings-of-power-2.0.0");
            given_project_package("sauron-1.0.0", "depends: rings-of-power < 3.0");
            given_project_package("frodo-1.0.0", "depends: rings-of-power = 1.0");

            given_dependency("depends: sauron");
            given_dependency("depends: frodo");

            when_resolving_packages();
        }
        [Test]
        public void common_compatible_version_is_resolved()
        {
            Resolve.SuccessfulPackages.Where(x => x.Identifier.Name == "rings-of-power")
                .ShouldHaveCountOf(1)
                .First().Packages.First().SemanticVersion.ShouldBe("1.0.0");
        }

        [Test]
        public void resolve_is_successful()
        {
            Resolve.IsSuccess.ShouldBeTrue();
        }
    }
}