using NUnit.Framework;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Configuration.simple
{
    public class ignored_properties_not_read : configuration<ignored_properties_not_persisted.ConfigWithIgnore>
    {
        public ignored_properties_not_read()
        {
            given_configuration_file("party", "human: Aragorn\r\nhobbit: Froddo Baggins");
            when_loading_configuration("party");
        }

        [Test]
        public void value_is_not_read()
        {
            Entry.Human.ShouldBeNull();
        }

        public class ConfigWithIgnore
        {
            public string Hobbit { get; set; }

            [OpenWrap.Configuration.Ignore]
            public string Human { get; set; }
        }
    }
}