using NUnit.Framework;
using OpenRasta.Client;
using OpenWrap.Configuration;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Configuration.simple
{
    internal class reading_from_file : configuration<reading_from_file.SimpleConfiguration>
    {
        public reading_from_file()
        {
            var configurationUri = Configurations.Addresses.BaseUri.Combine("test");
            given_configuration_text(configurationUri, "sauronsring: one to rule them all");
            when_loading_configuration(configurationUri);
        }

        [Test]
        public void value_is_read()
        {
            Entry.SauronsRing.ShouldBe("one to rule them all");
        }

        internal class SimpleConfiguration
        {
            public string SauronsRing { get; set; }
        }
    }
}