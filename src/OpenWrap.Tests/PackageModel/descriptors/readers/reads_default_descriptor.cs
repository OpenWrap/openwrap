using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap.Testing;
using Tests.contexts;

namespace OpenWrap.PackageModel.descriptors.readers
{
    public class reads_default_descriptor : descriptor_readers
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
}
