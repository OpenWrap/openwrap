using System;
using NUnit.Framework;
using OpenWrap.Configuration;
using OpenWrap.IO;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Configuration.simple
{
    public class encrypting_properties : configuration<encrypting_properties.EncryptingConfiguration>
    {
        public encrypting_properties()
        {
            given_configuration(new EncryptingConfiguration { RingLocation = "middle-earth" });
            when_saving_configuration("hobbiton");
        }

        [Test]
        public void roundtrips()
        {
            Entry.RingLocation.ShouldBe("middle-earth");
        }

        [Test]
        public void value_is_encrypted()
        {
            ConfigurationDirectory.GetFile("hobbiton").ReadString()
                .ShouldNotContain("middle-earth")
                .ShouldContain("ringlocation:");
        }

        public class EncryptingConfiguration
        {
            [Encrypt]
            public string RingLocation { get; set; }
        }
    }
}