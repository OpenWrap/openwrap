using NUnit.Framework;
using OpenWrap.IO;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Configuration.simple
{
    public class ignored_properties_not_persisted : configuration<ignored_properties_not_persisted.ConfigWithIgnore>
    {
        public ignored_properties_not_persisted()
        {
            given_configuration(new ConfigWithIgnore { Human = "Aragorn", Hobbit = "Froddo Baggins" });
            when_saving_configuration("party");
        }

        [Test]
        public void value_is_not_written()
        {
            ConfigurationDirectory.GetFile("party").ReadString().ShouldNotContain("human:").ShouldNotContain("Aragorn");
        }

        public class ConfigWithIgnore
        {
            public string Hobbit { get; set; }

            [OpenWrap.Configuration.Ignore]
            public string Human { get; set; }
        }
    }
}