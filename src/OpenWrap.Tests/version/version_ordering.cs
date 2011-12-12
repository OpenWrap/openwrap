using System.Collections.Generic;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Testing;

namespace Tests.version
{
    public class version_ordering : contexts.version
    {
        [Test]
        public void major_compared_numerically()
        {
            v("1.0.0").ShouldBeBefore(v("2.0.0"));
            v("1").ShouldBeBefore(v("2.0.0"));
        }
        [Test]
        public void minor_compared_numerically()
        {
            v("1.0.0").ShouldBeBefore(v("1.1.0"));            
        }
        [Test]
        public void patch_compared_numerically()
        {
            v("1.0.0").ShouldBeBefore(v("1.0.1"));
        }
        [Test]
        public void pre_has_lower_precedence_than_normal_version()
        {
            v("1.0.0-beta").ShouldBeBefore(v("1.0.0"));
        }
        [Test]
        public void pre_numeric_segment_compared_numerically()
        {
            v("1.0.0-2").ShouldBeBefore(v("1.0.0-10"));
        }
        [Test]
        public void pre_alpha_compared_by_ascii()
        {
            v("1.0.0-alpha").ShouldBeBefore(v("1.0.0-beta"));
        }
        [Test]
        public void longer_pre_takes_precedence()
        {
            v("1.0.0-alpha").ShouldBeBefore(v("1.0.0-alpha.1"));
        }
        [Test]
        public void pre_compares_each_segment_independently()
        {
            v("1.0.0-alpha.1").ShouldBeBefore(v("1.0.0-alpha.10"));
        }
        [Test]
        public void pre_numeric_segment_has_lower_precedence_to_alpha()
        {
            v("1.0.0-10").ShouldBeBefore(v("1.0.0-alpha"));
        }
        [Test]
        public void build_has_precedence_over_normal()
        {
            v("1.0.0").ShouldBeBefore(v("1.0.0+0.3.7"));
        }
        [Test]
        public void ordering_follows_specification()
        {
            var expected = new List<SemanticVersion>
            {
                v("1.0.0-alpha"),
                v("1.0.0-alpha.1"),
                v("1.0.0-beta.2"),
                v("1.0.0-beta.11"),
                v("1.0.0-rc.1"),
                v("1.0.0-rc.1+build.1"),
                v("1.0.0"),
                v("1.0.0+0.3.7"),
                v("1.3.7+build"),
                v("1.3.7+build.2.b8f12d7"),
                v("1.3.7+build.11.e0f985a")
            };
            var actual = new List<SemanticVersion>(expected);
            actual.Sort();
            actual.ShouldBe(expected);
        }
    }
}