using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap.Testing;

namespace OpenWrap.PackageModel.descriptors.readers
{
    public class reads_default_descriptor : contexts.descriptor_readers
    {
        public reads_default_descriptor()
        {
            given_descriptor("sauron.wrapdesc", "name: sauron");

            when_reading_all();
        }

        [Test]
        public void default_is_read()
        {
            Descriptors.ContainsKey(string.Empty).ShouldBeTrue();
            Descriptors[string.Empty]
                    .Check(x => x.File.ShouldBe(FileSystem.GetFile("sauron.wrapdesc")))
                    .Check(x => x.Value.Name.ShouldBe("sauron"));

        }
    }
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
    public class ignores_differently_named_descriptors : contexts.descriptor_readers
    {
        public ignores_differently_named_descriptors()
        {
            given_descriptor("sauron.wrapdesc", "name: sauron");
            given_descriptor("onering.tests.wrapdesc:", "description: blah");
            when_reading_all();

        }

        [Test]
        public void scoped_is_ignored()
        {
            Descriptors.ShouldHaveCountOf(1);

        }
    }
}
