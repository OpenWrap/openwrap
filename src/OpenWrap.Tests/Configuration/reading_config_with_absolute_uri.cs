using System;
using NUnit.Framework;
using OpenRasta.Client;
using OpenWrap.Configuration;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Configuration
{
    public class reading_config_with_absolute_uri : configuration<reading_config_with_absolute_uri.ConfigurationWithAttribute>
    {
        public reading_config_with_absolute_uri()
        {
            given_configuration_text((ConstantUris.URI_BASE + "/sauron").ToUri(), "key: value");
            when_loading_configuration();
        }

        [Test]
        public void value_is_read()
        {
            Entry.Key.ShouldBe("value");
        }

        [Path(ConstantUris.URI_BASE + "/sauron")]
        public class ConfigurationWithAttribute
        {
            public string Key { get; set; }
        }
    }
}