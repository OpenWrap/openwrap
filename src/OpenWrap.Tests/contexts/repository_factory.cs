using System;
using System.IO;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using OpenRasta.Client;
using OpenRasta.Client.Memory;
using OpenWrap.Repositories;
using OpenWrap.Testing;
using Tests.Repositories.factories;

namespace Tests.contexts
{
    public abstract class http : context
    {
        protected MemoryHttpClient Client = new MemoryHttpClient();

        protected void given_remote_resource(string uri, string mediaType, string content, string username = null, string password = null)
        {
            Client.Resources[uri.ToUri()] = new MemoryResource(new MediaType(mediaType), new MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
                Username = username,
                Password = password
            };
        }

        protected void given_remote_resource(string uri, MemoryResource resource)
        {
            Client.Resources[uri.ToUri()] = resource;
        }

        protected void given_remote_resource(string uri,
                                             Expression<Func<IClientRequest, IClientResponse>> handler)
        {
            Client.Resources[uri.ToUri()] = new MemoryResource
            {
                    Operations =
                            {
                                    { handler.Parameters[0].Name, handler.Compile() }
                            }
            };
        }
    }

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