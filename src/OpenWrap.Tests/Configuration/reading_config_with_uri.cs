using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenRasta.Client;
using OpenWrap;
using OpenWrap.Configuration;
using OpenWrap.Testing;
using Tests;

namespace Tests.Configuration
{
    public class reading_config_with_uri : contexts.configuration<ConfigurationWithAttribute>
    {
        public reading_config_with_uri()
        {
            given_configuration_text((ConstantUris.URI_BASE + "/sauron").ToUri(), "key: value");
            when_loading_configuration();
        }

        [Test]
        public void value_is_read()
        {
            Entry.Key.ShouldBe("value");
        }
    }

    [PathUri(ConstantUris.URI_BASE + "/sauron")]
    public class ConfigurationWithAttribute
    {
        public string Key { get; set; }
    }
}