using NUnit.Framework;
using OpenWrap.Commands.Core;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.configuration
{
    public class set_proxy_href : set_configuration
    {
        public set_proxy_href()
        {
            when_executing_command("-proxy https://server/");
        }

        [Test]
        public void configuration_is_set()
        {
            Configuration.ProxyHref.ShouldBe("https://server/");
        }

        [Test]
        public void message_notifies_of_change()
        {
            Results.ShouldHaveOne<ConfigurationUpdated>()
                .Types.ShouldContain("proxy-href");
        }
    }
}