using NUnit.Framework;
using OpenWrap;
using OpenWrap.Testing;

namespace Tests.Dependencies.parser
{
    public class anchored_with_version : dependency_parser_context
    {
        public anchored_with_version()
        {
            given_dependency("depends: nhibernate = 2.0 anchored");
        }
        [Test]
        public void the_anchor_is_found()
        {
            Declaration.Name.ShouldBe("nhibernate");
            Declaration.Anchored.ShouldBeTrue();
            Declaration.IsFulfilledBy(SemanticVersion.TryParseExact("2.0.0.0"));
        }
    }
}