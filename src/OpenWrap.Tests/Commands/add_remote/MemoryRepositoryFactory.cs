using System;
using OpenWrap.Repositories;

namespace OpenWrap.Tests.Commands.Remote.Add
{
    public class MemoryRepositoryFactory : IRemoteRepositoryFactory
    {
        public Func<string, IPackageRepository> FromUserInput = input=>null;
        IPackageRepository IRemoteRepositoryFactory.FromUserInput(string identifier)
        {
            return FromUserInput(identifier);
        }

        public Func<string, IPackageRepository> FromToken = input => null;
        IPackageRepository IRemoteRepositoryFactory.FromToken(string token)
        {
            return FromToken(token);
        }
    }
}