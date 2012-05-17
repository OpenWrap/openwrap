using NUnit.Framework;
using OpenWrap;
using OpenWrap.PackageModel;

namespace Tests.Dependencies.nuked
{
    public class when_resolving_build_dependency_against_nuked_build : global::Tests.Dependencies.contexts.nuked_package_resolution
    {
        public when_resolving_build_dependency_against_nuked_build()
        {
            given_package_version("2.1.0");
            given_nuked_package_version("2.1.1");
            given_dependency(new EqualVersionVertex(SemanticVersion.TryParseExact("2.1.1")));
            when_resolving();
        }

        [Test]
        public void the_nuked_revision_is_returned()
        {
            ResolvedVersion.Equals("2.1.1");
        }
    }
}