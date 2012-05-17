using System.Linq;
using NUnit.Framework;
using OpenWrap.PackageModel;
using OpenWrap.Testing;

namespace Tests.Dependencies.parser
{
    public class name : dependency_parser_context
    {
        public name()
        {
            given_dependency("depends: nhibernate");
        }

        [Test]
        public void the_name_is_parsed()
        {
            Declaration.Name.ShouldBe("nhibernate");
        }

        [Test]
        public void the_version_is_any()
        {
            Declaration.VersionVertices.First().ShouldBeOfType<AnyVersionVertex>()
                .ShouldAccept("0.0.0.0");
        }
    }
}