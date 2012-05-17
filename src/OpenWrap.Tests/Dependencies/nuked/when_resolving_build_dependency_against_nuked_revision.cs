using NUnit.Framework;
using OpenWrap;
using OpenWrap.PackageModel;

namespace Tests.Dependencies.nuked
{
    public class when_resolving_build_dependency_against_nuked_revision : global::Tests.Dependencies.contexts.nuked_package_resolution
    {
        public when_resolving_build_dependency_against_nuked_revision()
        {
            given_package_version("1.0.0.0");
            given_nuked_package_version("1.0.0.1");
            given_dependency(new EqualVersionVertex(SemanticVersion.TryParseExact("1.0.0")));
            when_resolving();
        }

        [Test]
        public void the_non_nuked_revision_is_returned()
        {
            ResolvedVersion.Equals("1.0.0.0");
        }
    }
}