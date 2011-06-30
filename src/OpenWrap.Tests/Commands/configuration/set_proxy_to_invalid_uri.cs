using NUnit.Framework;
using OpenWrap.Commands.Core;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.configuration
{
    public class set_proxy_to_invalid_uri : set_configuration
    {
        public set_proxy_to_invalid_uri()
        {
            when_executing_command("-proxy unknownString");
        }

        [Test]
        public void error_is_reported()
        {
            Results.ShouldHaveOne<InvalidProxy>()
                .Href.ShouldBe("unknownString");
        }
    }
}