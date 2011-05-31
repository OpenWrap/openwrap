using NUnit.Framework;
using OpenWrap.Configuration;
using OpenWrap.Testing;

namespace Tests.Commands.add_remote
{
    class adding_publish_to_existing_with_unknown_repository_type : contexts.add_remote
    {
        public adding_publish_to_existing_with_unknown_repository_type()
        {
            given_remote_configuration(new RemoteRepositories { { "iron-hills", new RemoteRepository { FetchRepository = "[indexed]unknown" } } });
            
            when_executing_command("iron-hills -publish somewhere");
        }

        [Test]
        public void an_error_is_returned()
        {
            Results.ShouldHaveError();
        }

        [Test]
        public void configuration_is_not_modified()
        {
            Remotes["iron-hills"].PublishRepositories.ShouldBeEmpty();

        }
    }
}