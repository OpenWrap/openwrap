using NUnit.Framework;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Configuration.simple
{
    public class readonly_properties_not_read : configuration<readonly_properties_not_read.Config>
    {
        public readonly_properties_not_read()
        {
            given_configuration_file("party", "hobbit: Froddo Baggins");
            when_loading_configuration("party");
        }

        [Test]
        public void value_is_not_read()
        {
            Entry.Hobbit.ShouldBe("Sam");
        }

        public class Config
        {
            public Config()
            {
                Hobbit = "Sam";
            }

            public string Hobbit { get; private set; }
        }
    }
}