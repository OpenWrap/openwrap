using NUnit.Framework;
using OpenWrap.Commands.Messages;
using OpenWrap.Testing;

namespace Tests.Commands.remote.set
{
    [TestFixture("secundus -username sauron")]
    [TestFixture("secundus -password sarumansucks")]
    public class changing_credentials_missing : contexts.set_remote
    {
        public changing_credentials_missing(string input)
        {
            given_remote_config("secundus");
            when_executing_command(input);
        }

        [Test]
        public void error_is_returned()
        {
            Results.ShouldHaveOne<IncompleteCredentials>();
        }
    }
}