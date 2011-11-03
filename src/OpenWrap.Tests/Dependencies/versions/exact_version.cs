using NUnit.Framework;
using OpenWrap;
using OpenWrap.PackageModel;

namespace Tests.Dependencies.versions
{
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
            return new EqualVersionVertex(versionvertice.ToVersion());
        }
    }
}