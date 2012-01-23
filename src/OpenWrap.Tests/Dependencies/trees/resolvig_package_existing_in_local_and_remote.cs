using System;
using System.Linq;
using NUnit.Framework;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Dependencies.trees
{
    public class resolvig_package_existing_in_local_and_remote : dependency_resolver_context
    {
        public resolvig_package_existing_in_local_and_remote()
        {
            given_remote1_package("rings-of-power-1.1.0");
            given_project_package("rings-of-power-1.0.0");
            given_dependency("depends: rings-of-power");

            when_resolving_packages();
        }

        [Test]
        public void finds_highest_version_number_across_repositories()
        {
            Resolve.IsSuccess.ShouldBeTrue();
            var dependency = Resolve.SuccessfulPackages.First();

            dependency.Packages.ShouldNotBeEmpty().First()
                .Source.ShouldBe(RemoteRepository);
            dependency.Packages.ShouldNotBeEmpty()
                .First().SemanticVersion.ShouldBe("1.1.0");
        }
    }
}