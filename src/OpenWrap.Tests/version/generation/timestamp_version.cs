using NUnit.Framework;

namespace Tests.version.generation
{
    public class timestamp_version: contexts.version
    {
        [Test]
        public void patch()
        {
            ver_build("1.0.*").ShouldMatch(@"1\.0\.\d+");
        }
        [Test]
        public void build()
        {
            ver_build("1.0.0.*").ShouldMatch(@"1\.0\.0\+\d+");
            ver_build("1.0.0+*").ShouldMatch(@"1\.0\.0\+\d+");
        }
        [Test]
        public void build_and_patch_have_same_number()
        {
            ver_build("1.0.*+build.*").ShouldMatch(@"1\.0\.(?<generated>\d+)\+build.\k<generated>");
        }
    }
}