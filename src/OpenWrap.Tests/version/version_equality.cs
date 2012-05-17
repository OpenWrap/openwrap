using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.version
{
    public class version_equality : contexts.version
    {
        [Test]
        public void on_numeric_values()
        {
            v("1.0.0").ShouldBe(v("1.0.0"));
        }
        [Test]
        public void on_missing_values()
        {
            v("1").ShouldBe(v("1.0.0"));
        }
        [Test]
        public void on_build_and_pre()
        {
            v("1.0.0-pre+build").ShouldBe(v("1.0.0-pre+build"));
        }
    }
}