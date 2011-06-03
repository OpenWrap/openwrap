using NUnit.Framework;
using OpenWrap.Commands.Errors;
using OpenWrap.Testing;

namespace Tests.Commands.publish_wrap
{
    public class publishing_to_unknown_remote : contexts.publish_wrap
    {
        public publishing_to_unknown_remote()
        {
            given_currentdirectory_package("sauron", "1.0.0.123");
            when_executing_command("-remote mordor -path sauron-1.0.0.123.wrap");
        }

        [Test]
        public void command_fails()
        {
            Results.ShouldHaveOne<UnknownRemoteName>()
                .Name.ShouldBe("mordor");
        }
    }
}