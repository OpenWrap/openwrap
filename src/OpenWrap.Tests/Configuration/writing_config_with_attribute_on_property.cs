using NUnit.Framework;
using OpenWrap.Configuration;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Configuration
{
    public class writing_config_with_attribute_on_property : configuration<writing_config_with_attribute_on_property.Config>
    {
        public writing_config_with_attribute_on_property()
        {
            given_configuration(new Config { RingName = "The one" });
            when_saving_configuration("sauron");
        }

        [Test]
        public void value_is_roundtripped()
        {
            Entry.RingName.ShouldBe("The one");
        }
        public class Config
        {
            [Key("key")]
            public string RingName { get; set; }
        }
    }
}