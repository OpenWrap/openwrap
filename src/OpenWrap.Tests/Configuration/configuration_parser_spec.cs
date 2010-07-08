using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap.Configuration;
using OpenWrap.Testing;

namespace OpenWrap.Tests.Configuration
{
    public class configuration_parser_spec : Testing.context
    {
        [Test]
        public void section_without_name_is_parsed()
        {
            new ConfigurationParser().Parse("[section]")
                .ShouldHaveCountOf(1)
                .First().ShouldBeOfType<ConfigurationSection>()
                .Check(x => x.Name.ShouldBeEmpty())
                .Check(x => x.Type.ShouldBe("section"));
        }
        [Test]
        public void section_with_name_is_parsed()
        {
            new ConfigurationParser().Parse("[section named]")
                .ShouldHaveCountOf(1)
                .First().ShouldBeOfType<ConfigurationSection>()
                .Check(x => x.Name.ShouldBe("named"))
                .Check(x => x.Type.ShouldBe("section"));
        }
        [Test]
        public void lineis_parsed()
        {
            new ConfigurationParser().Parse("name = value")
                .ShouldHaveCountOf(1)
                .First().ShouldBeOfType<ConfigurationLine>()
                .Check(x => x.Name.ShouldBe("name"))
                .Check(x => x.Value.ShouldBe("value"));
        }
        [Test]
        public void line_is_parsed_within_section()
        {

            new ConfigurationParser().Parse("[section mastaba]\r\nmaterial = bricks")
                .ShouldHaveCountOf(1)
                .First().ShouldBeOfType<ConfigurationSection>()
                .Check(x => x.Name.ShouldBe("mastaba"))
                .Check(x => x.Type.ShouldBe("section"))
                .Lines.ShouldHaveCountOf(1).First()
                .Check(x => x.Name.ShouldBe("material"))
                .Check(x => x.Value.ShouldBe("bricks"));
        }
    }
}
