using NUnit.Framework;
using OpenWrap.Commands.Remote.Messages;
using OpenWrap.Testing;

namespace Tests.Commands.set_remote
{
    public class changing_credentials_on_existing : contexts.set_remote
    {
        public changing_credentials_on_existing()
        {
            given_remote_config("secundus", publishTokens: "[memory]location");
            when_executing_command("secundus -username sauron -password sarumansucks");
        }

        [Test]
        public void fetch_password_is_set()
        {
            ConfiguredRemotes["secundus"].FetchRepository
                .Password.ShouldBe("sarumansucks");
        }

        [Test]
        public void fetch_username_is_set()
        {
            ConfiguredRemotes["secundus"].FetchRepository
                .Username.ShouldBe("sauron");
        }

        [Test]
        public void message_notifies_update()
        {
            Results.ShouldHaveOne<RemoteUpdated>()
                .Name.ShouldBe("secundus");
        }

        [Test]
        public void publish_password_set()
        {
            ConfiguredRemotes["secundus"].PublishRepositories
                .ShouldHaveAll(x => x.Password == "sarumansucks");
        }

        [Test]
        public void publish_username_set()
        {
            ConfiguredRemotes["secundus"].PublishRepositories
                .ShouldHaveAll(x => x.Username == "sauron");
        }
    }
}