using System.Linq;
using NUnit.Framework;
using OpenWrap.Commands;
using OpenWrap.Tests;
using Tests.Commands.contexts;

namespace publish_wrap_specifications
{
    public class auth_on_non_supporting_remote : publish_wrap
    {
        public auth_on_non_supporting_remote()
        {
            given_remote_repository("mordor", factory: _=>_.CanAuthenticate = false);
            
            given_currentdirectory_package("sauron", "1.0.0.123");
            when_executing_command("-remote mordor -path sauron-1.0.0.123.wrap -user frodo -pwd ring");
        }

        [Test]
        public void command_warns()
        {
            Results.ShouldContain<Warning>();
        }
    }
}