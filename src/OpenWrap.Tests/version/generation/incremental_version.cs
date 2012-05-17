using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.version.generation
{
    public class incremental_version : contexts.version
    {
        [Test]
        public void patch()
        {
            ver_build("1.0.#").ShouldBe("1.0.0");
            last_build.ShouldBe("0");
        }
        [Test]
        public void build()
        {
            ver_build("1.0.0.#").ShouldBe("1.0.0+0");
            last_build.ShouldBe("0");

            ver_build("1.0.0+#", 0).ShouldBe("1.0.0+1");
            last_build.ShouldBe("1");
        }
        [Test]
        public void build_and_patch_have_same_number()
        {
            ver_build("1.0.#+build.#", 5).ShouldBe("1.0.6+build.6");
        }
    }
}