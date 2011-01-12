using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap.PackageModel;
namespace OpenWrap.PackageModel.descriptors.scoped
{

    public class setting_inexistant_single_value
        : contexts.scoped_descriptor
    {
        public setting_inexistant_single_value()
        {
            given_descriptor("name: one-ring");
            given_scoped_descriptor();

            ScopedDescriptor.Anchored = true;

            when_writing();
        }

        [Test]
        public void value_is_set_in_scoped()
        {
            scoped_descriptor_should_be("anchored: true");
        }
        [Test]
        public void value_is_not_set_in_default()
        {
            descriptor_should_be("name: one-ring");
        }
    }
    public class adding_to_multiline_value
        : contexts.scoped_descriptor
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
