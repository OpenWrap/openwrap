using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Dependencies.parser
{
    public class anchored_when_repeated : dependency_parser_context
    {
        public anchored_when_repeated()
        {
            given_dependency("depends: anchored anchored");
        }
        [Test]
        public void the_anchor_is_found()
        {
            Declaration.Name.ShouldBe("anchored");
            Declaration.Anchored.ShouldBeTrue();

        }
    }
}