using NUnit.Framework;
using OpenWrap.PackageModel.descriptors.contexts;

namespace package_descriptor_specs
{
    public class editing_single_value : descriptor
    {
        public editing_single_value()
        {
            given_descriptor("anchored: false", "unknown: value");
            Descriptor.Anchored = true;
            when_writing();
        }

        [Test]
        public void value_is_edited_in_place()
        {
            descriptor_should_be("anchored: true\r\nunknown: value");
        }
    }
}