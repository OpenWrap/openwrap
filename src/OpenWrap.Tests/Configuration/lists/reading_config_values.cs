using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Configuration;
using OpenWrap.Testing;
using Tests;

namespace Tests.Configuration.lists
{
    public class reading_values : contexts.configuration<reading_values.ConfigSection>
    {
        public reading_values()
        {
            given_configuration_file("config", "value: one\r\nvalue: two");
            when_loading_configuration("config");
        }

        [Test]
        public void list_is_read()
        {
            Entry.Values.First().Input.ShouldBe("one");
            Entry.Values.ElementAt(1).Input.ShouldBe("two");

        }
        public class ConfigSection
        {
            [Key("value")]
            public ICollection<ConfigItem> Values { get; set; }
        }
        public class ConfigItem
        {
            public ConfigItem(string input)
            {
                Input = input;
            }

            public string Input { get; set; }
            public override string ToString()
            {
                return Input;
            }
        }
    }
}