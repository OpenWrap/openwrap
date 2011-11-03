using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.source_dir
{
    public class custom_source : context_source_dir
    {
        public custom_source()
        {
            given_descriptor(
                "name: one-ring",
                "version: 1.0.0",
                "directory-structure: {source: code}");
            given_folder("code");
            when_finding_source();
        }

        [Test]
        public void directory_is_foud()
        {
            source.ShouldBe(root.GetDirectory("code"));
        }
    }
}