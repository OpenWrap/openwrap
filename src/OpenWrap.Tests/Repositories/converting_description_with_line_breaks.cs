using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Repositories
{
    public class converting_description_with_line_breaks : contexts.nuspec

    {
        public converting_description_with_line_breaks()
        {
            given_spec("one-ring", "1.0.0", "sauron, saruman", "One ring\r\nto \n\nrule \nthem all.");
            
        }  
        [Test]
        public void description_is_converted()
        {
            Descriptor.Description.ShouldBe("One ring to rule them all.");
        }
    }
}