using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.resolving_dependency_trees
{
    class resolving_namespaced_dependency : contexts.dependency_resolver_context
    {
        public resolving_namespaced_dependency()
        {
            given_remote1_package("beta/one-ring");
            given_dependency("depends: beta/one-ring");

            when_resolving_packages();
        }

        [Test]
        public void resolve_is_successful()
        {
            Resolve.IsSuccess.ShouldBeTrue();
        }
    }
}
