using NUnit.Framework;
using OpenWrap.Testing;

namespace OpenWrap.PackageModel.descriptors.readers
{
    public class ignores_differently_named_descriptors : contexts.descriptor_readers
    {
        public ignores_differently_named_descriptors()
        {
            given_descriptor("sauron.wrapdesc", "name: sauron");
            given_descriptor("onering.tests.wrapdesc", "description: blah");
            when_reading_all();
        }

        [Test]
        public void scoped_is_ignored()
        {
            Descriptors.ShouldHaveCountOf(1);
        }
    }
}