using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.source_dir
{
    public class custom_source_not_found : context_source_dir
    {
        public custom_source_not_found()
        {
            given_descriptor(
                "name: one-ring",
                "version: 1.0.0",
                "directory-structure: {source: code}");
            
            when_finding_source();
        }

        [Test]
        public void directory_is_foud()
        {
            source.ShouldBeNull();
        }
    }
}