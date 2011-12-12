using System;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Testing;

namespace Tests.version.generation
{
    public class flat_version : contexts.version
    {
        [Test]
        public void three_components()
        {
            ver_build("1.0.0").ShouldBe("1.0.0");
        }
        [Test]
        public void four_components()
        {
            ver_build("1.0.0.0").ShouldBe("1.0.0+0");
        }
        [Test]
        public void build_component()
        {
            ver_build("1.0.0+build.0").ShouldBe("1.0.0+build.0");
        }
        [Test]
        public void pre_component()
        {
            ver_build("1.0.0-alpha").ShouldBe("1.0.0-alpha");
        }
    }
}