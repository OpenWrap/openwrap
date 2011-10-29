using System;
using System.Net;
using OpenWrap.Repositories;

namespace Tests
{
    public class MemoryRepositoryFactory : IRemoteRepositoryFactory
    {
        public Func<string,NetworkCredential, IPackageRepository> FromUserInput = (input,cred)=>null;
        IPackageRepository IRemoteRepositoryFactory.FromUserInput(string userInput, NetworkCredential credentials)
        {
            return FromUserInput(userInput, credentials);
        }

        public Func<string, IPackageRepository> FromToken = input => null;
        IPackageRepository IRemoteRepositoryFactory.FromToken(string token)
        {
            return FromToken(token);
        }
    }
}