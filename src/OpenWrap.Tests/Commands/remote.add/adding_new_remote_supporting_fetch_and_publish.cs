﻿using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands.Remote;
using OpenWrap.Commands.Remote.Messages;
using OpenWrap.Repositories;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.remote.add
{
    public class adding_new_remote_supporting_fetch_and_publish : remote_command<AddRemoteCommand>
    {
        public adding_new_remote_supporting_fetch_and_publish()
        {
            given_remote_factory_memory();
            when_executing_command("iron-hills http://lotr.org/iron-hills");
        }

        [Test]
        public void outputs_success_message()
        {
            Results.ShouldHaveOne<RemoteAdded>()
                .Name.ShouldBe("iron-hills");
        }

        [Test]
        public void remote_has_correct_fetch_token()
        {
            ConfiguredRemotes["iron-hills"].FetchRepository.Token.ShouldBe("[memory]http://lotr.org/iron-hills");
        }

        [Test]
        public void remote_has_correct_publish_token()
        {
            ConfiguredRemotes["iron-hills"].PublishRepositories.ShouldHaveCountOf(1).First().Token.ShouldBe("[memory]http://lotr.org/iron-hills");
        }

        [Test]
        public void remote_has_priority_of_one()
        {
            ConfiguredRemotes["iron-hills"].Priority.ShouldBe(1);
        }

        [Test]
        public void remote_is_added_to_the_list()
        {
            ConfiguredRemotes.ContainsKey("iron-hills").ShouldBeTrue();
        }
    }
}