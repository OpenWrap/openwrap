using NUnit.Framework;
using OpenWrap;
using OpenWrap.PackageModel;

namespace Tests.Dependencies.versions
{
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
            return new GreaterThanOrEqualVersionVertex(versionvertice.ToVersion());
        }
    }
}