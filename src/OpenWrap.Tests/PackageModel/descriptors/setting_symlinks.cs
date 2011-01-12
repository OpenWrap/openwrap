using NUnit.Framework;
using OpenWrap.PackageModel.descriptors.contexts;

namespace package_descriptor_specs
{
    public class setting_symlinks : descriptor
    {
        public setting_symlinks()
        {
            given_descriptor();
            Descriptor.UseSymLinks = false;
            when_writing();
        }
        [Test]
        public void symlinks_are_disabled()
        {
            descriptor_should_be("use-symlinks: false");
        }
    }
}