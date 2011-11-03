using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Repositories.NuGet;
using OpenWrap.Testing;

namespace Tests.Repositories
{
    public class maven_style_version_definition : OpenWrap.Testing.context
    {
        [Test]
        public void default_version()
        {
            version("1.0.0").ShouldBe(">= 1.0 and < 1.1");
        }

        [Test]
        public void default_version_major_minor()
        {
            version("1.0").ShouldBe("= 1.0");
        }

        public void less_than_or_equal()
        {
            version("(,1.0]").ShouldBe("<= 1.0");
        }
        public void less_than()
        {
            version("(,1.0)").ShouldBe("< 1.0");
        }

        [Test]
        public void exact_version()
        {
            version("[1.0]").ShouldBe("= 1.0");
        }

        [Test]
        public void more_than_or_equal()
        {
            version("[1.0,)").ShouldBe(">= 1.0");
        }

        [Test]
        public void more_than()
        {
            version("(1.0,)").ShouldBe("> 1.0");
        }

        [Test]
        public void less_and_less()
        {
            version("(1.0,2.0)").ShouldBe("> 1.0 and < 2.0");
        }

        [Test]
        public void less_equal_and_less_equal()
        {
            version("[1.0,2.0]").ShouldBe(">= 1.0 and <= 2.0");
        }
        string version(string s)
        {
            return NuConverter.ConvertNuGetVersionRange(s).Select(x=>x.ToString()).JoinString(" and ");
        }
    }
}