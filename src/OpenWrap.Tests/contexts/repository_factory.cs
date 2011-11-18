using System;
using System.Net;
using OpenRasta.Client;
using OpenWrap.Repositories;

namespace Tests.contexts
{
    public class repository_factory<T, TRepository> : http where T : IRemoteRepositoryFactory where TRepository : IPackageRepository
    {
        protected T Factory;
        protected IPackageRepository Repository;

        public repository_factory(Func<IHttpClient, T> builder)
        {
            Factory = builder(Client);
        }

        protected void when_building_from_token(string token)
        {
            Repository = (TRepository)Factory.FromToken(token);
        }

        protected void when_detecting(string userInput, string username = null, string password=null)
        {
            Repository = Factory.FromUserInput(userInput, username != null ? new NetworkCredential(username,password) : null);
        }
    }
}