using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.version
{
    public class version_conversion : contexts.version
    {
        [Test]
        public void convert_components()
        {
            v("1.0").ToVersion().ToString().ShouldBe("1.0");
            v("1.0.0").ToVersion().ToString().ShouldBe("1.0.0");
            v("1.0.0.0").ToVersion().ToString().ShouldBe("1.0.0.0");
        }
        [Test]
        public void pre_removed()
        {
            v("1.0.0-alpha").ToVersion().ToString().ShouldBe("1.0.0");
        }
        [Test]
        public void numeric_build_converted()
        {
            v("1.0.0+1").ToVersion().ToString().ShouldBe("1.0.0.1");
        }
        [Test]
        public void alpha_build_converted()
        {
            v("1.0.0+build").ToVersion().ToString().ShouldBe("1.0.0");            
        }
    }
}