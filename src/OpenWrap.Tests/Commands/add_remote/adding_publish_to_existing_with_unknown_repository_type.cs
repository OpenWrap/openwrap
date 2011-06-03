﻿using NUnit.Framework;
using OpenWrap.Commands.Remote.Messages;
using OpenWrap.Configuration.Remotes;
using OpenWrap.Testing;

namespace Tests.Commands.add_remote
{
    class adding_publish_to_existing_with_unknown_repository_type : contexts.add_remote
    {
        public adding_publish_to_existing_with_unknown_repository_type()
        {
            given_remote_configuration(new RemoteRepositories { { "iron-hills", new RemoteRepository { FetchRepository = new RemoteRepositoryEndpoint { Token = "[indexed]unknown" } } } });

            when_executing_command("iron-hills -publish somewhere");
        }

        [Test]
        public void an_error_is_returned()
        {
            Results.ShouldHaveOne<UnknownEndpointType>()
                .Path.ShouldBe("somewhere");
        }

        [Test]
        public void configuration_is_not_modified()
        {
            StoredRemotesConfig["iron-hills"].PublishRepositories.ShouldBeEmpty();
        }
    }
}