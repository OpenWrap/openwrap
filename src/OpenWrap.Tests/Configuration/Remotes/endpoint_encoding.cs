using System;
using NUnit.Framework;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenWrap.Configuration;
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
            var confMan = new DefaultConfigurationManager(new InMemoryFileSystem().GetDirectory(@"c:\config").MustExist());
            confMan.Save(new RemoteRepositories
            {
                new RemoteRepository
                {
                    Name = "iron-hills",
                    FetchRepository = new RemoteRepositoryEndpoint { Token = token, Username = username, Password = password }
                }
            });

            confMan.Load<RemoteRepositories>()["iron-hills"]
                .Check(x => x.FetchRepository.Token.ShouldBe(token))
                .Check(x => x.FetchRepository.Username.ShouldBe(username))
                .Check(x => x.FetchRepository.Password.ShouldBe(password));
        }
    }
}