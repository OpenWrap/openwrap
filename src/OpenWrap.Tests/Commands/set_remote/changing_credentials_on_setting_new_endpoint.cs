using System.Linq;
using NUnit.Framework;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.set_remote
{
    public class changing_credentials_on_setting_new_endpoint : contexts.set_remote
    {
        public changing_credentials_on_setting_new_endpoint()
        {
            given_remote_factory(input => new InMemoryRepository(input));
            given_remote_config("secundus");
            when_executing_command("secundus -href http://localhost -username sauron -password sarumansucks");
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
        public void publish_password_set()
        {
            ConfiguredRemotes["secundus"].PublishRepositories
                .Single().Password.ShouldBe("sarumansucks");
        }

        [Test]
        public void publish_username_set()
        {
            ConfiguredRemotes["secundus"].PublishRepositories
                .Single().Username.ShouldBe("sauron");
        }
    }
}