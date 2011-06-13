using System;
using NUnit.Framework;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Configuration.Remotes
{
    public class remotes_with_publish_only : openwrap_context
    {
        public remotes_with_publish_only()
        {
            given_remote_config("sauron", fetchToken: null, publishTokens: "[memory]somewhere");
        }

        [Test]
        public void persisted_has_no_fetch()
        {
            ConfiguredRemotes["sauron"].FetchRepository.ShouldBeNull();
        }
    }
}