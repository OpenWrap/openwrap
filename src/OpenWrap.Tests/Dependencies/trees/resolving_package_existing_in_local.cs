using NUnit.Framework;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Dependencies.trees
{
    public class resolving_package_existing_in_local : dependency_resolver_context
    {
        public resolving_package_existing_in_local()
        {
            given_project_package("rings-of-power-1.0.0");
            given_dependency("depends: rings-of-power");

            when_resolving_packages();
        }

        [Test]
        public void package_is_resolved()
        {
            Resolve.IsSuccess.ShouldBeTrue();
            Resolve.SuccessfulPackages.ShouldHaveCountOf(1);
        }
    }
}