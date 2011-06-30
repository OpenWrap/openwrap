using NUnit.Framework;
using OpenWrap.IO;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Configuration.simple
{
    public class complex_properties_constructor_value_persisted : configuration<complex_properties_constructor_value_persisted.Config>
    {
        public complex_properties_constructor_value_persisted()
        {
            given_configuration(new Config { Entry = new ConfigEntry("Froddo Baggins") { Type = "hobbit" } });
            when_saving_configuration("party");
        }

        [Test]
        public void values_are_written()
        {
            ConfigurationDirectory.GetFile("party").ReadString().ShouldBe("entry: Froddo Baggins; type=hobbit\r\n");
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