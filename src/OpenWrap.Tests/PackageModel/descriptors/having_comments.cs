using System.Linq;
using NUnit.Framework;
using OpenWrap.contexts;
using OpenWrap.Testing;

namespace package_descriptor_specs
{
    public class having_comments : descriptor
    {
        public having_comments()
        {
            given_descriptor("   # Love will find a way, if you want it to", "anchored: false", "# What I meant to say is that build, blabla", "build: value");
            when_writing();
        }

        [Test]
        public void lines_are_preserved()
        {
            Descriptor.Anchored.ShouldBe(false);
            Descriptor.Build.First().ShouldBe("value");
        }

        [Test]
        public void comments_are_preserved()
        {
            descriptor_should_be("# Love will find a way, if you want it to\r\nanchored: false\r\n# What I meant to say is that build, blabla\r\nbuild: value");
        }
    }
}