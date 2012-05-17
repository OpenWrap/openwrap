using System.Linq;
using NUnit.Framework;
using OpenWrap.PackageModel;
using OpenWrap.Testing;

namespace Tests.Dependencies.parser
{
    public class name_with_version : dependency_parser_context
    {
        public name_with_version()
        {
            given_dependency("depends: nhibernate >= 2.1");
        }

        [Test]
        public void the_name_is_parsed()
        {
            Declaration.Name.ShouldBe("nhibernate");
        }

        [Test]
        public void the_version_vertice_is_of_correct_type()
        {
            Declaration.VersionVertices
                .First().ShouldBeOfType<GreaterThanOrEqualVersionVertex>()
                .ShouldAccept("2.1.0.0")
                .ShouldAccept("3.0");
        }
    }
}