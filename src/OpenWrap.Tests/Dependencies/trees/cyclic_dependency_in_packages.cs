using System.Linq;
using NUnit.Framework;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Dependencies.trees
{
    public class cyclic_dependency_in_packages : dependency_resolver_context
    {
        public cyclic_dependency_in_packages()
        {
            given_system_package("evil-1.0.0", "depends: evil");

            given_dependency("depends: evil");

            when_resolving_packages();
        }

        [Test]
        public void package_is_present()
        {
            Resolve.SuccessfulPackages.Count().ShouldBe(1);
            Resolve.SuccessfulPackages.First()
                .Check(x => x.Identifier.Name.ShouldBe("evil"))
                .Check(x => x.Packages.ShouldHaveCountOf(1))
                .Check(x => x.Identifier.Version.ShouldBe("1.0.0"));

        }

        [Test]
        public void resolve_is_successful()
        {
            Resolve.IsSuccess.ShouldBeTrue();
        }
    }
}