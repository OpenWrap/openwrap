using NUnit.Framework;
using OpenWrap.Commands.Errors;
using OpenWrap.Tests;
using Tests.Commands.contexts;

namespace publish_wrap_specifications
{
    public class publishing_to_unknown_remote : publish_wrap
    {
        public publishing_to_unknown_remote()
        {
            given_currentdirectory_package("sauron", "1.0.0.123");
            when_executing_command("-remote mordor -path sauron-1.0.0.123.wrap");
        }

        [Test]
        public void command_fails()
        {
            Results.ShouldContain<UnknownRemoteRepository>();
        }
    }
}
