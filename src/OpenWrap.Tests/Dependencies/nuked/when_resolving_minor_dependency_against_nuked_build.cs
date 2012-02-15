using NUnit.Framework;
using OpenWrap;
using OpenWrap.PackageModel;
using OpenWrap.Testing;

namespace Tests.Dependencies.nuked
{
    public class when_resolving_minor_dependency_against_nuked_build : global::Tests.Dependencies.contexts.nuked_package_resolution
    {
        public when_resolving_minor_dependency_against_nuked_build()
        {
            given_package_version("2.1.0.0");
            given_nuked_package_version("2.1.1.0");
            given_dependency(new EqualVersionVertex(SemanticVersion.TryParseExact("2.1")));
            when_resolving();
        }

        [Test]
        public void the_non_nuked_revision_is_returned()
        {
            ResolvedVersion.ShouldBe("2.1.0+0");
        }
    }
}