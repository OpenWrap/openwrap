using System.Linq;
using NUnit.Framework;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Dependencies.trees
{
    public class resolving_unavailable_sub_dependencies : dependency_resolver_context
    {
        public resolving_unavailable_sub_dependencies()
        {
            given_system_package("rings-of-power-1.0.0", "depends: unknown");
            given_dependency("depends: rings-of-power");

            when_resolving_packages();
        }
        [Test]
        public void resolution_fails()
        {
            Resolve.IsSuccess.ShouldBeFalse();
        }
        [Test]
        public void missing_package_is_described()
        {
            Resolve.MissingPackages.ShouldHaveCountOf(1)
                .First().Identifier.Name.ShouldBe("unknown");
        }
    }
}