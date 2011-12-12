using NUnit.Framework;
using OpenWrap;
using OpenWrap.PackageModel;

namespace Tests.Dependencies.versions
{
    [TestFixture]
    public class aproximately_greater : version_vertice_context
    {
        [TestCase("2.0.0", "2.0.0.0")]
        [TestCase("2.0.0", "2.0.0.1")]
        [TestCase("2.0.0", "2.0.1.1")]
        [TestCase("2.0.0", "2.0.0")]
        [TestCase("2.0.0", "2.0.1")]
        [TestCase("2.0", "2.0")]
        [TestCase("2", "2.0")]
        [TestCase("2", "2.1")]
        public void positive_matches(string vertex, string version)
        {
            should_match(vertex, version);
        }

        [TestCase("2.0", "1.0")]
        [TestCase("2.0.0", "1.0.0")]
        [TestCase("2.0.0", "1.0.0.0")]
        [TestCase("2.0.0", "2.1")]
        [TestCase("2.0", "3.0")]
        [TestCase("2.0", "1.0.1")]
        [TestCase("2", "1.0.1")]
        [TestCase("2", "3.0.0")]
        public void negative_matches(string vertex, string version)
        {
            should_not_match(vertex, version);
        }

        protected override VersionVertex CreateVertex(string versionvertice)
        {
            return new AproximatelyGreaterVersionVertex(versionvertice.ToSemVer());
        }
    }
}