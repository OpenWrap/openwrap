using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Build.assembly_info
{
    public class none : context_assembly_info
    {
        public none()
        {
            given_descriptor();
            when_generating_assembly_info();
        }

        [Test]
        public void no_file_generated()
        {
            assembly_info_file.Exists.ShouldBeFalse();
        }
    }
}