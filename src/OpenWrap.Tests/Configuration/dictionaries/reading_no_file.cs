using NUnit.Framework;
using OpenRasta.Client;
using OpenWrap.Configuration;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Configuration.dictionaries
{
    internal class reading_no_file : configuration<reading_no_file.Config>
    {
        public reading_no_file()
        {
            when_loading_configuration("unknown");
        }

        [Test]
        public void a_default_value_is_returned()
        {
            Entry.ShouldBeSameInstanceAs(Config.Default);
        }

        internal class Config
        {
            public static Config Default = new Config();
        }
    }
}