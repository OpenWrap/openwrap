using NUnit.Framework;
using OpenWrap.Configuration;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Configuration.dictionaries
{
    internal class when_two_dictionary_values_with_same_name_are_present : configuration<RemoteRepositories>
    {
        public when_two_dictionary_values_with_same_name_are_present()
        {
            given_configuration_text(Configurations.Addresses.RemoteRepositories,
                                     "[remoterepository openwrap]\r\nhref:http://wraps.openwrap.org\r\n[remoterepository openwrap]\r\nhref:http://default.openwrap.org");

            when_loading_configuration(Configurations.Addresses.RemoteRepositories);
        }

        [Test]
        public void an_error_is_triggered()
        {
            Error.ShouldBeOfType<InvalidConfigurationException>();
        }
    }
}