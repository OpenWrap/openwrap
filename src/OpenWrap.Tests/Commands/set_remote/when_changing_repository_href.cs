using System;
using NUnit.Framework;
using OpenWrap.Repositories;
using OpenWrap.Testing;
using OpenWrap.Tests.Commands.Remote.Set.context;

namespace OpenWrap.Tests.Commands.Remote.Set
{
    public class when_changing_repository_href : set_remote
    {
        public when_changing_repository_href()
        {
            given_remote_factory(input => new InMemoryRepository(input));
            when_executing_command("secundus -href http://awesomereps.net");
        }

        [Test]
        public void the_second_repository_has_new_href()
        {
            TryGetRepository("secundus").FetchRepository.ShouldBe("[memory]http://awesomereps.net");
        }
    }
}