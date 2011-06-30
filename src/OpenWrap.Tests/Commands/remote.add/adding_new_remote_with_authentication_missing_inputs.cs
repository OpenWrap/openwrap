using NUnit.Framework;
using OpenWrap.Commands.Messages;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.remote.add
{
    [TestFixture("iron-hills http://sauron -username forlong.the.fat")]
    [TestFixture("iron-hills http://sauron -password lossarnach")]
    public class adding_new_remote_with_authentication_missing_inputs : contexts.add_remote
    {
        public adding_new_remote_with_authentication_missing_inputs(string input)
        {
            given_remote_factory_memory();
            when_executing_command(input);
        }

        [Test]
        public void error_is_returned()
        {
            Results.ShouldHaveOne<IncompleteCredentials>();
        }
    }
}