using NUnit.Framework;
using OpenWrap.Commands.Core;
using OpenWrap.Testing;
using Tests.Commands.contexts;

namespace Tests.Commands.configuration
{
    [TestFixture("https://sauron:one-ring@server/", "one-ring")]
    [TestFixture("https://sauron:one-ring:@server/", "one-ring:")]
    [TestFixture("https://sauron:one-ring%3A@server/", "one-ring:")]
    public class set_proxy_password : set_configuration
    {
        readonly string _exepctedPassword;

        public set_proxy_password(string uri, string exepctedPassword)
        {
            _exepctedPassword = exepctedPassword;
            when_executing_command("-proxy " + uri);
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
        public void message_notifies_of_password()
        {
            Results.ShouldHaveOne<ConfigurationUpdated>()
                .Types.ShouldContain("proxy-password");
        }

        [Test]
        public void message_notifies_of_username()
        {
            Results.ShouldHaveOne<ConfigurationUpdated>()
                .Types.ShouldContain("proxy-username");
        }

        [Test]
        public void password_is_set()
        {
            Configuration.ProxyPassword.ShouldBe(_exepctedPassword);
        }
    }
}