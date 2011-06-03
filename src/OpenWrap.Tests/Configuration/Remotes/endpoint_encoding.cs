using System;
using NUnit.Framework;
using OpenWrap.Configuration.Remotes;
using OpenWrap.Testing;

namespace Tests.Configuration.Remotes
{
    class endpoint_encoding : context
    {
        [TestCase("token", null, null)]
        [TestCase("token", "username", "password")]
        [TestCase("to;ken", "user;name", "password")]
        [TestCase("token", "username", "password")]
        [TestCase("tok=en", "username", "password")]
        [TestCase("token", "usern=ame", "password")]
        [TestCase("token", "username", "pas=sword")]
        [TestCase("tok\\en", "username", "password")]
        [TestCase("token", "usernam\\e", "password")]
        [TestCase("token", "username", "pa\\;=ssword")]
        public void roundtrips(string token, string username, string password)
        {
            new RemoteRepositoryEndpoint(new RemoteRepositoryEndpoint { Token = token, Username = username, Password = password }.ToString())
                .Check(x => x.Token.ShouldBe(token))
                .Check(x => x.Username.ShouldBe(username))
                .Check(x => x.Password.ShouldBe(password));
        }
    }
}