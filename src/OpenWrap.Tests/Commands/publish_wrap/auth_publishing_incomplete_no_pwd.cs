using NUnit.Framework;
using OpenWrap.Commands.Errors;
using OpenWrap.Tests;
using Tests.Commands.contexts;

namespace publish_wrap_specifications
{
    public class auth_publishing_incomplete_no_pwd : publish_wrap
    {
        public auth_publishing_incomplete_no_pwd()
        {
            given_remote_repository("mordor");
            given_currentdirectory_package("sauron", "1.0.0.123");
            when_executing_command("-remote", "mordor", "-path", "sauron-1.0.0.123.wrap", "-user", "frodo");
        }

        [Test]
        public void command_fails()
        {
            Results.ShouldContain<IncompleteAuthentication>();
        }
    }
}