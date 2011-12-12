using System.Linq;
using NUnit.Framework;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Dependencies.trees
{
    public class resolving_package_from_remote_repository : dependency_resolver_context
    {
        public resolving_package_from_remote_repository()
        {
            given_remote1_package("rings-of-power-1.0.0");
            given_dependency("depends: rings-of-power");

            when_resolving_packages();
        }

        [Test]
        public void dependency_on_remote_package_is_resolved()
        {
            Resolve.IsSuccess.ShouldBeTrue();
            Resolve.SuccessfulPackages.First().Packages.ShouldNotBeEmpty()
                .First()
                .Source.ShouldBe(RemoteRepository);
        }
    }
}