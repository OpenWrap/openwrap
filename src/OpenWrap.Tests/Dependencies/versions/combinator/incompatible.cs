using System;
using NUnit.Framework;
using OpenWrap.Collections;
using OpenWrap.Testing;

namespace Tests.Dependencies.versions.combinator
{
    public class incompatible : context
    {
        [Test]
        public void hard_min_and_hard_max()
        {
            combining("< 2.0", "> 3.0").ShouldBeNull();
        }
        [Test]
        public void soft_min_and_version()
        {
            combining(">= 2.0", "= 1.0").ShouldBeNull();
        }
        [Test]
        public void soft_max_and_version()
        {
            combining("<= 2.0", "= 3.0").ShouldBeNull();
        }
        [Test]
        public void equal()
        {
            combining("= 1.0", "= 1.1").ShouldBeNull();
        }
        [Test]
        public void absolute_equal()
        {
            combining("≡ 1.0.0+0", "≡ 1.0.0+1").ShouldBeNull();

        }
    }
}