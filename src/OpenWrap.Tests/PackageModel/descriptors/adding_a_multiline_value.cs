using NUnit.Framework;
using OpenWrap.PackageModel;
using Tests.contexts;

namespace package_descriptor_specs
{
    public class adding_a_multiline_value : descriptor
    {
        public adding_a_multiline_value()
        {
            given_descriptor("depends: ered-luin");
            Descriptor.Dependencies.Add(new PackageDependency("ered-mithrin"));

            when_writing();
        }

        [Test]
        public void value_is_appended()
        {
            descriptor_should_be("depends: ered-luin", "depends: ered-mithrin");
        }
    }
}