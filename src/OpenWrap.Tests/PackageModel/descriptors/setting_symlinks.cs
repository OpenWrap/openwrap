using NUnit.Framework;
using OpenWrap.contexts;

namespace package_descriptor_specs
{
    public class setting_symlinks : descriptor
    {
        public setting_symlinks()
        {
            given_descriptor();
            Descriptor.UseSymLinks = true;
            when_writing();
        }
        [Test]
        public void symlinks_are_enabled()
        {
            descriptor_should_be("use-symlinks: true");
        }
    }
}