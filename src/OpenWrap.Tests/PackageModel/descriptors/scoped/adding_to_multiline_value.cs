using NUnit.Framework;
using Tests.contexts;

namespace OpenWrap.PackageModel.descriptors.scoped
{
    public class adding_to_multiline_value
            : scoped_descriptor
    {
        public adding_to_multiline_value()
        {
            given_descriptor("name: one-ring");
            given_scoped_descriptor();

            ScopedDescriptor.Build.Add("files; file=c:\\tmp");

            when_writing();

        }

        [Test]
        public void value_is_added_to_scoped()
        {
            scoped_descriptor_should_be("build: files; file=c:\\tmp");
        }

        [Test]
        public void value_is_not_added_to_default()
        {
            descriptor_should_be("name: one-ring");
        }
    }
}