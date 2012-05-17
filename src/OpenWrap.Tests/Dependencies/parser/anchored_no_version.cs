using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Dependencies.parser
{
    public class anchored_no_version : dependency_parser_context
    {
        public anchored_no_version()
        {
            given_dependency("depends: nhibernate anchored");
        }
        [Test]
        public void the_anchor_is_found()
        {
            Declaration.Name.ShouldBe("nhibernate");
            Declaration.Anchored.ShouldBeTrue();
            
        }
    }
}