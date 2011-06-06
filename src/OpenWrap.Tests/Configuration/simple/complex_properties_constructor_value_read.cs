using NUnit.Framework;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Configuration.simple
{
    public class complex_properties_constructor_value_read : configuration<complex_properties_constructor_value_read.Config>
    {
        public complex_properties_constructor_value_read()
        {
            given_configuration_file("party", "entry: Froddo Baggins; type=\"hobbit\"");
            when_loading_configuration("party");
        }

        [Test]
        public void constructor_value_is_parsed()
        {
            Entry.Entry.Name.ShouldBe("Froddo Baggins");
        }

        [Test]
        public void property_value_is_parsed()
        {
            Entry.Entry.Type.ShouldBe("hobbit");
        }

        public class Config
        {
            public ConfigEntry Entry { get; set; }
        }

        public class ConfigEntry
        {
            public ConfigEntry(string name)
            {
                Name = name;
            }

            [OpenWrap.Configuration.Ignore]
            public string Name { get; private set; }

            public string Type { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }
    }
}