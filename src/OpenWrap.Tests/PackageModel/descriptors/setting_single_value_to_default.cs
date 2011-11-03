using NUnit.Framework;
using Tests.contexts;

namespace package_descriptor_specs
{
    public class setting_single_value_to_default : descriptor
    {
        public setting_single_value_to_default()
        {
            given_descriptor("anchored: true");
            Descriptor.Anchored = false;
            when_writing();
        }

        [Test]
        public void line_is_removed()
        {
            descriptor_should_be("");
        }
    }
}