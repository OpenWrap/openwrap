using NUnit.Framework;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Configuration.simple
{
    class reading_from_file : configuration<reading_from_file.SimpleConfiguration>
    {
        public reading_from_file()
        {
            given_configuration_file("test", "sauronsring: one to rule them all");
            when_loading_configuration("test");
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