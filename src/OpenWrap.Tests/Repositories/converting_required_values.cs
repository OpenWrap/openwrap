using NUnit.Framework;
using OpenWrap;
using OpenWrap.Testing;

namespace Tests.Repositories
{
    public class converting_required_values : contexts.nuspec
    {
        public converting_required_values()
        {
            given_spec("one-ring", "1.0.0", "sauron, saruman", "One ring to rule them all.");
        }

        [Test]
        public void name_is_converted()
        {
            Descriptor.Name.ShouldBe("one-ring");
        }

        [Test]
        public void version_is_converted()
        {
            Descriptor.SemanticVersion.ShouldBe("1.0.0".ToSemVer());
        }


        //[Test]
        //public void authors_are_converted()
        //{
            
        //}
    }
}