using System;
using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Testing;

namespace Tests.Dependencies.parser
{
    public class multiple_versions : dependency_parser_context
    {
        public multiple_versions()
        {
            given_dependency("depends: nhibernate >= 2.1 and < 3.0");
        }

        [Test]
        public void the_versions_are_processed()
        {
            Declaration.VersionVertices.Count().ShouldBe(2);

            Declaration.IsFulfilledBy(SemanticVersion.TryParseExact("2.1.0.0")).ShouldBeTrue();
            Declaration.IsFulfilledBy(SemanticVersion.TryParseExact("3.0.0.0")).ShouldBeFalse();
        }
    }
}