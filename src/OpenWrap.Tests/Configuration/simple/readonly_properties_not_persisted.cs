using NUnit.Framework;
using OpenWrap.IO;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Configuration.simple
{
    public class readonly_properties_not_persisted : configuration<readonly_properties_not_persisted.Config>
    {
        public readonly_properties_not_persisted()
        {
            given_configuration(new Config());
            when_saving_configuration("party");
        }

        [Test]
        public void value_is_not_written()
        {
            ConfigurationDirectory.GetFile("party").
                ReadString().ShouldNotContain("hobbit:").ShouldNotContain("Froddo Baggins");
        }

        public class Config
        {
            public Config()
            {
                Hobbit = "Froddo Baggins";
            }

            public string Hobbit { get; private set; }
        }
    }
}