using System.Linq;
using NUnit.Framework;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Dependencies.trees
{
    public class resolving_package_from_system_repository : dependency_resolver_context
    {
        public resolving_package_from_system_repository()
        {
            given_system_package("rings-of-power-1.0.0");
            given_dependency("depends: rings-of-power");

            when_resolving_packages();
        }

        [Test]
        public void system_package_is_resolved()
        {
            Resolve.IsSuccess.ShouldBeTrue();
            Resolve.SuccessfulPackages.First().Packages.ShouldNotBeEmpty()
                .First().Source.ShouldBe(SystemRepository);
        }
    }
}