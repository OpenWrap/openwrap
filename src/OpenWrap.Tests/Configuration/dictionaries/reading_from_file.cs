using System;
using NUnit.Framework;
using OpenWrap.Configuration;
using OpenWrap.Testing;
using Tests.contexts;

namespace Tests.Configuration.dictionaries
{
    internal class reading_from_file : configuration<RemoteRepositories>
    {
        static readonly Uri repositoryUri = new Uri("http://configuration.openwrap.org/repository");

        public reading_from_file()
        {
            given_configuration_text("remote-repositories",
                                     "[remoterepository openwrap]\r\nfetch: token=\"http://wraps.openwrap.org\"\r\n[remoterepository]\r\nfetch:token=\"http://default.openwrap.org\"");

            when_loading_configuration("remote-repositories");
        }

        [Test]
        public void entries_in_dictionary_have_their_properties_set()
        {
            Entry["openwrap"].FetchRepository.Token.ShouldBe("http://wraps.openwrap.org");
        }

        [Test]
        public void sections_with_no_name_have_an_empty_key()
        {
            Entry.ContainsKey(string.Empty).ShouldBeTrue();
        }

        [Test]
        public void the_name_in_the_section_is_the_key_in_the_dictionary()
        {
            Entry.ContainsKey("openwrap").ShouldBeTrue();
        }
    }
}