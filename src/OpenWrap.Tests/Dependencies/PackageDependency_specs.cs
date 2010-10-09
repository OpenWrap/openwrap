using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap.Dependencies;
using OpenWrap.Testing;

namespace OpenWrap.Tests.Dependencies
{
    public class PackageDependency_specs : context
    {
        [Test]
        public void no_version()
        {
            ShouldParse("sauron");

        }
        [Test]
        public void exact_version()
        {
            ShouldParse("sauron = 1.0");
        }
        [Test]
        public void multiple_conditions()
        {
            ShouldParse("sauron >= 2.0 and < 3.0");
        }
        [Test]
        public void anchored_is_written_after_exact_version()
        {
            var name = "sauron = 1.0 anchored";
            ShouldParse(name);
        }
        [Test]
        public void anchored_is_written_after_version_range()
        {
            var name = "sauron >= 2.0 and < 1.0 anchored";
            ShouldParse(name);
        }
        [Test]
        public void anchored_is_written_after_no_version()
        {
            var name = "sauron anchored";
            ShouldParse(name);
        }

        [Test]
        public void order_of_tags_is_preserved()
        {
            ShouldParse("sauron anchored unknown content");
        }
        string ShouldParse(string name)
        {
            return DependsParser.ParseDependsInstruction("depends: " + name)
                    .Dependencies.First().ToString().ShouldBe(name);
        }
    }
}
