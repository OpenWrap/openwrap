using System;
using System.Linq;
using NUnit.Framework;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Dependencies.trees
{
    public class resolving_latest_package_in_system_with_outdated_remote_version : dependency_resolver_context
    {
        public resolving_latest_package_in_system_with_outdated_remote_version()
        {
            given_system_package("one-ring-1.0.0.1");
            given_remote1_package("one-ring-1.0.0.0");

            given_dependency("depends: one-ring");

            when_resolving_packages();
        }

        [Test]
        public void resolve_is_successful()
        {
            Resolve.IsSuccess.ShouldBeTrue();
        }

        [Test]
        public void the_package_is_installed_from_system()
        {
            Resolve.SuccessfulPackages.ShouldHaveCountOf(1)
                .First()
                .Check(x => x.Packages.First().Source.ShouldBe(SystemRepository))
                .Check(x => x.Packages.First().Version.ShouldBe("1.0.0.1"));
        }
    }
}