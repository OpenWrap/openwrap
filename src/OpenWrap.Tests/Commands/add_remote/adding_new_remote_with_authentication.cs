using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Commands.Messages;
using OpenWrap.Repositories;
using OpenWrap.Testing;
using Tests;

namespace Tests.Commands.add_remote
{
    [TestFixture("iron-hills http://sauron -username forlong.the.fat")]
    [TestFixture("iron-hills http://sauron -password lossarnach")]
    public class adding_new_remote_with_authentication_missing_inputs : contexts.add_remote
    {
        public adding_new_remote_with_authentication_missing_inputs(string input)
        {
            given_remote_factory(userInput => new InMemoryRepository(userInput));
            when_executing_command(input);
        }

        [Test]
        public void error_is_returned()
        {
            Results.ShouldHaveOne<MissingCredentials>();
        }
    }
    public class adding_new_remote_with_authentication : contexts.add_remote
    {
        public adding_new_remote_with_authentication()
        {
        }
    }
}