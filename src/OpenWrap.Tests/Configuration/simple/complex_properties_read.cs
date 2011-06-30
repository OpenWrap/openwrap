using NUnit.Framework;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Configuration.simple
{
    public class complex_properties_read : configuration<complex_properties_read.Config>
    {
        public complex_properties_read()
        {
            given_configuration_file("party", "entry: name=\"Froddo Baggins\"");
            when_loading_configuration("party");
        }

        [Test]
        public void value_read()
        {
            Entry.Entry.Name.ShouldBe("Froddo Baggins");
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