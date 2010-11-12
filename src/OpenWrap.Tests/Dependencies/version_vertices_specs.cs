using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap.Dependencies;
using OpenWrap.Testing;

namespace OpenWrap.Tests.Dependencies
{
    public abstract class version_vertice_context
    {
        protected void should_match(string versionvertice, string version)
        {
            SpecExtensions.ShouldBeTrue(CreateVertex(versionvertice)
                                          .IsCompatibleWith(version.ToVersion()));
        }

        protected void should_not_match(string versionvertice, string version)
        {
            SpecExtensions.ShouldBeFalse(CreateVertex(versionvertice)
                                           .IsCompatibleWith(version.ToVersion()));
        }

        protected abstract VersionVertex CreateVertex(string versionvertice);
    }

    [TestFixture]
    public class exact_version : version_vertice_context
    {
        [TestCase("1.0.0","1.0.0.234")]
        [TestCase("1.0.0", "1.0.0")]
        [TestCase("1.0", "1.0.1.2")]
        [TestCase("1.0", "1.0.1")]
        [TestCase("1.0", "1.0")]
        public void positive_matches(string vertex, string version)
        {
            should_match(vertex, version);
        }

        [TestCase("1.0.0", "1.0.1.234")]
        [TestCase("1.0.0", "1.0.1")]
        [TestCase("1.0.0", "1.0")]
        [TestCase("1.1.0", "1.1.1")]
        [TestCase("1.1.0", "1.2")]
        [TestCase("1.1.0", "2.0")]
        [TestCase("1.1", "1.2.3.4")]
        [TestCase("1.1", "1.2.3")]
        [TestCase("1.1", "1.2")]
        [TestCase("1.1.0", "1.1")]
        public void negative_matches(string vertex, string version)
        {
            base.should_not_match(vertex, version);
        }

        protected override VersionVertex CreateVertex(string versionvertice)
        {
            return new ExactVersionVertex(versionvertice.ToVersion());
        }
    }
    [TestFixture]
    public class greater_than_or_equal : version_vertice_context
    {

        [TestCase("1.0.0", "1.0.0.0")]
        [TestCase("1.0.0", "1.0.0.1")]
        [TestCase("1.0.0", "1.0.1.1")]
        [TestCase("1.0.0", "1.1.1.1")]
        [TestCase("1.0.0", "1.0.0")]
        [TestCase("1.0", "1.0.0.0")]
        [TestCase("1.0", "1.0.0.1")]
        [TestCase("1.0", "1.0.1.1")]
        [TestCase("1.0", "1.0.0")]
        [TestCase("1.0", "1.0.1")]
        [TestCase("1.0", "1.0")]
        [TestCase("1.1", "2.0")]
        public void positive_matches(string vertex, string version)
        {
            should_match(vertex, version);
        }
        
        [TestCase("2.1.0", "1.0.1.0")]
        [TestCase("2.1.0", "1.1.1.0")]
        [TestCase("2.1.0", "1.0.1")]
        [TestCase("2.1.0", "1.1.1")]
        [TestCase("2.1", "1.1.0.0")]
        [TestCase("2.1", "1.1.0")]
        [TestCase("2.1", "1.1")]
        [TestCase("2.1.0", "2.0.1.1")]
        [TestCase("2.1.0", "2.0.1")]
        [TestCase("2.1.0", "2.0")]
        public void negative_matches(string vertex, string version)
        {
            should_not_match(vertex, version);
        }
        protected override VersionVertex CreateVertex(string versionvertice)
        {
            return new GreaterThenOrEqualVersionVertex(versionvertice.ToVersion());
        }
    }
    [TestFixture]
    public class less_than : version_vertice_context
    {
        [TestCase("2.0.0", "1.0.0.0")]
        [TestCase("2.0.0", "1.0.0.1")]
        [TestCase("2.0.0", "1.0.1.1")]
        [TestCase("2.0.0", "1.1.1.1")]
        [TestCase("2.0.0", "1.0.0")]
        [TestCase("2.0.1", "2.0.0")]
        [TestCase("2.0", "1.0.0.0")]
        [TestCase("2.0", "1.0.0.1")]
        [TestCase("2.0", "1.0.1.1")]
        [TestCase("2.0", "1.0.0")]
        [TestCase("2.0", "1.0.1")]
        [TestCase("2.0", "1.0")]
        public void positive_matches(string vertex, string version)
        {
            should_match(vertex, version);
        }


        [TestCase("2.0.0", "2.0.0.1")]
        [TestCase("2.0.0", "2.0.1")]
        [TestCase("2.0.0", "2.1")]
        [TestCase("2.0", "2.0.0.1")]
        public void negative_matches(string vertex, string version)
        {
            should_not_match(vertex, version);
        }

        protected override VersionVertex CreateVertex(string versionvertice)
        {
            return new LessThanVersionVertex(versionvertice.ToVersion());
        }
    }
}
