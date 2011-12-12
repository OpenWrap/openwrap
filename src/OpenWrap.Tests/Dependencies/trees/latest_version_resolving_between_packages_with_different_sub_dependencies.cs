using NUnit.Framework;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Dependencies.trees
{
    public class latest_version_resolving_between_packages_with_different_sub_dependencies : dependency_resolver_context
    {
        public latest_version_resolving_between_packages_with_different_sub_dependencies()
        {
            given_system_package("ring-of-power-1.0.0");
            given_system_package("ring-of-power-1.0.1");
            given_remote1_package("frodo-1.0.0", "depends: ring-of-power = 1.0.0");
            given_remote1_package("frodo-1.0.1", "depends: ring-of-power = 1.0.1");

            given_dependency("depends: frodo");

            when_resolving_packages();
        }
        [Test]
        public void resolve_is_successful()
        {
            Resolve.IsSuccess.ShouldBeTrue();
        }
    }
}