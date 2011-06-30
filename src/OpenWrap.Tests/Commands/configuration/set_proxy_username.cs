using NUnit.Framework;
using OpenWrap.Commands.Core;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.configuration
{
    public class set_proxy_username : set_configuration
    {
        public set_proxy_username()
        {
            when_executing_command("-proxy https://sauron@server/");
        }

        [Test]
        public void configuration_is_set()
        {
            Configuration.ProxyHref.ShouldBe("https://server/");
        }

        [Test]
        public void message_notifies_of_href()
        {
            Results.ShouldHaveOne<ConfigurationUpdated>()
                .Types.ShouldContain("proxy-href");
        }

        [Test]
        public void message_notifies_of_username()
        {
            Results.ShouldHaveOne<ConfigurationUpdated>()
                .Types.ShouldContain("proxy-username");
        }

        [Test]
        public void username_is_set()
        {
            Configuration.ProxyUsername.ShouldBe("sauron");
        }
    }
}