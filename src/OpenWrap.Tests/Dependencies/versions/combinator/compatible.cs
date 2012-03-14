using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Dependencies.versions.combinator
{
    public class compatible : context
    {
        [Test]
        public void hard_max_bounds()
        {
            combining("< 2.0", "< 2.5").ShouldBe("< 2.0");
        }
        [Test]
        public void soft_and_hard_max_bounds()
        {
            combining("<= 2.0", "< 2.0").ShouldBe("< 2.0");
        }
        [Test]
        public void soft_min_bound()
        {
            combining(">= 3.0", ">= 3.1").ShouldBe(">= 3.1");
        }
        [Test]
        public void hard_min_bound()
        {
            combining("> 3.0", "> 3.1").ShouldBe("> 3.1");
        }
        [Test]
        public void semver_and_soft_min()
        {
            combining("~> 3.0", ">= 3.1").ShouldBe(">= 3.1 and < 4");
        }
        [Test]
        public void equality()
        {
            combining("= 1.0", "= 1.0.1").ShouldBe("= 1.0.1");
        }
        [Test]
        public void absolute_equality()
        {
            combining("≡ 1.0.0+0", "").ShouldBe("≡ 1.0.0+0");
        }
        [Test]
        public void equality_and_absolute_equality()
        {
            combining("≡ 1.0.0+0", "= 1.0").ShouldBe("≡ 1.0.0+0");
        }
        [Test]
        public void equality_and_min()
        {
            combining("= 1.0", ">= 1.0").ShouldBe("= 1.0");
        }
        [Test]
        public void none_and_equal()
        {
            combining("", "= 1.0").ShouldBe("= 1.0");

        }
    }
}