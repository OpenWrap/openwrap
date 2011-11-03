using NUnit.Framework;
using OpenWrap;
using OpenWrap.PackageModel;

namespace Tests.Dependencies.versions
{
    [TestFixture]
    public class less_than_or_equal : version_vertice_context
    {
        [TestCase("2.0.0", "1.0.0.0")]
        [TestCase("2.0.0", "1.0.0.1")]
        [TestCase("2.0.0", "1.0.1.1")]
        [TestCase("2.0.0", "1.1.1.1")]
        [TestCase("2.0.0", "2.0.0.1")]
        [TestCase("2.0.0", "1.0.0")]
        [TestCase("2.0.0", "2.0.0")]
        [TestCase("2.0.1", "2.0.0")]
        [TestCase("2.0", "1.0.0.0")]
        [TestCase("2.0", "1.0.0.1")]
        [TestCase("2.0", "1.0.1.1")]
        [TestCase("2.0", "2.0.0.1")]
        [TestCase("2.0", "1.0.0")]
        [TestCase("2.0", "1.0.1")]
        [TestCase("2.0", "2.0")]
        [TestCase("2.0", "1.0")]
        public void positive_matches(string vertex, string version)
        {
            should_match(vertex, version);
        }


        [TestCase("2.0.0", "2.0.1")]
        [TestCase("2.0.0", "2.1")]
        [TestCase("2.0.0", "3.0")]
        public void negative_matches(string vertex, string version)
        {
            should_not_match(vertex, version);
        }

        protected override VersionVertex CreateVertex(string versionvertice)
        {
            return new LessThanOrEqualVersionVertex(versionvertice.ToVersion());
        }
    }
}