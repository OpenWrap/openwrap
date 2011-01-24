using NUnit.Framework;
using OpenWrap.Testing;

namespace OpenWrap.PackageModel.descriptors.readers
{
    public class reads_scoped_descirptor : contexts.descriptor_readers
    {
        public reads_scoped_descirptor()
        {
            given_descriptor("sauron.wrapdesc", "name: sauron");
            given_descriptor("sauron.tests.wrapdesc", "depends: nunit");

            when_reading_all();

        }

        [Test]
        public void scoped_is_read()
        {
            Descriptors.ContainsKey("tests").ShouldBeTrue();
            Descriptors["tests"].Value.Dependencies.ShouldHaveCountOf(1);
        }
    }
}