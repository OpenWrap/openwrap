using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap.Commands;
using OpenWrap.Testing;

namespace Tests.source_dir
{
    [TestFixture("src")]
    [TestFixture("source")]
    public class default_for_source : context_source_dir
    {
        string expected;

        public default_for_source(string folderName)
        {
            given_descriptor(
                "name: one-ring",
                "version: 1.0.0");
            given_folder(expected = folderName);
            when_finding_source();
        }

        [Test]
        public void directory_is_foud()
        {
            source.ShouldBe(root.GetDirectory(expected));
        }
    }
}
