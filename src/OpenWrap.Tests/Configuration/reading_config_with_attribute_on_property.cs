using NUnit.Framework;
using OpenRasta.Client;
using OpenWrap.Configuration;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Configuration
{
    public class reading_config_with_attribute_on_property : configuration<reading_config_with_attribute_on_property.ConfigurationWithPropertyAttrib>
    {
        public reading_config_with_attribute_on_property()
        {
            given_configuration_text((ConstantUris.URI_BASE + "/sauron").ToUri(), "key: value");
            when_loading_configuration();
        }

        [Test]
        public void value_is_read()
        {
            Entry.RingName.ShouldBe("value");
        }

        [PathUri(ConstantUris.URI_BASE + "/sauron")]
        public class ConfigurationWithPropertyAttrib
        {
            [Key("key")]
            public string RingName { get; set; }
        }
    }
}