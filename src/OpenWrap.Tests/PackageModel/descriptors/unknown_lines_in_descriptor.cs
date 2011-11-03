using NUnit.Framework;
using Tests.contexts;

namespace package_descriptor_specs
{
    public class unknown_lines_in_descriptor : descriptor
    {
        public unknown_lines_in_descriptor()
        {
            given_descriptor("anchored: false", "my-custom-value: something", "build: value");
            Descriptor.Anchored = true;
            Descriptor.Build.Clear();
            Descriptor.Build.Add("newValue");
            when_writing();
        }

        [Test]
        public void value_is_preserved()
        {
            descriptor_should_be("anchored: true", "my-custom-value: something", "build: newValue");
        }
    }
}