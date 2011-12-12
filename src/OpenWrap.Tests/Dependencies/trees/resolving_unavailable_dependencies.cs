using System.Linq;
using NUnit.Framework;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Dependencies.trees
{
    public class resolving_unavailable_dependencies : dependency_resolver_context
    {
        public resolving_unavailable_dependencies()
        {
            given_dependency("depends: rings-of-power");

            when_resolving_packages();
        }

        [Test]
        public void has_no_successful_package()
        {
            Resolve.SuccessfulPackages.ShouldHaveCountOf(0);
        }
        [Test]
        public void has_missing_package()
        {
            Resolve.MissingPackages.ShouldHaveCountOf(1)
                .First().Identifier.Name.ShouldBe("rings-of-power");
        }

        [Test]
        public void resolution_fails()
        {
            Resolve.IsSuccess.ShouldBeFalse();
        }
    }
}