using NUnit.Framework;
using OpenWrap.Configuration;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Configuration.simple
{
    public class decrypting_property_invalid_value : configuration<decrypting_property_invalid_value.Config>
    {
        public decrypting_property_invalid_value()
        {
            given_configuration_file("somewhere", "encrypted: notencrypteddata\r\ndecrypted: value");
            when_loading_configuration("somewhere");
        }

        [Test]
        public void value_is_ignored()
        {
            Entry.Encrypted.ShouldBeNull();
        }

        public class Config
        {
            public string Decrypted { get; set; }

            [Encrypt]
            public string Encrypted { get; set; }
        }
    }
}