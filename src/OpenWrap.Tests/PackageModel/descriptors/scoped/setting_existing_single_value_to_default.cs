using NUnit.Framework;
using Tests.contexts;

namespace OpenWrap.PackageModel.descriptors.scoped
{
    public class setting_existing_single_value_to_default
            : scoped_descriptor
    {
        public setting_existing_single_value_to_default()
        {
            given_descriptor("anchored: true");
            given_scoped_descriptor();

            ScopedDescriptor.Anchored = false;

            when_writing();
        }

        [Test]
        public void value_is_set_in_scoped()
        {
            scoped_descriptor_should_be("anchored: false");
        }

        [Test]
        public void value_is_preserved_in_default()
        {
            descriptor_should_be("anchored: true");
        }
    }
}