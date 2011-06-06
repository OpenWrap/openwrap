using System;
using NUnit.Framework;
using OpenWrap.IO;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Configuration.simple
{
    public class complex_properties_persisted : configuration<complex_properties_persisted.Config>
    {
        public complex_properties_persisted()
        {
            given_configuration(new Config { Entry = new ConfigEntry { Name = "Froddo Baggins" } });
            when_saving_configuration("party");
        }

        [Test]
        public void configuration_is_written()
        {
            ConfigurationDirectory.GetFile("party").ReadString().ShouldContain("entry: name=Froddo Baggins");
        }

        public class Config
        {
            public ConfigEntry Entry { get; set; }
        }

        public class ConfigEntry
        {
            public string Name { get; set; }
        }
    }
}