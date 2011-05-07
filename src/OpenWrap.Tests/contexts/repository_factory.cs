using System;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using OpenRasta.Client;
using OpenRasta.Client.Memory;
using OpenWrap.Repositories;
using OpenWrap.Testing;
using Tests.Repositories.factories;

namespace Tests.contexts
{
    public class repository_factory<T> : context where T : IRemoteRepositoryFactory
    {
        protected MemoryHttpClient Client = new MemoryHttpClient();
        protected T Factory;
        protected IPackageRepository Repository;

        public repository_factory(Func<IHttpClient, T> builder)
        {
            Factory = builder(Client);
        }

        protected void when_detecting(string userInput)
        {
            Repository = Factory.FromUserInput(userInput);
        }
        protected void when_building_from_token(string token)
        {
            Repository = Factory.FromToken(token);
        }
        protected void given_remote_resource(string uri, string mediaType, string content)
        {
            Client.Resources[uri.ToUri()] = new MemoryResource(new MediaType(mediaType), new MemoryStream(Encoding.UTF8.GetBytes(content)));
        }
        protected void given_remote_resource(string uri, MemoryResource resource)
        {
            Client.Resources[uri.ToUri()] = resource;
        }
        protected void given_remote_resource(string uri, 
            Expression<Func<IClientRequest,IClientResponse>> handler)
        {
            Client.Resources[uri.ToUri()] = new MemoryResource
            {
                    Operations =
                            {
                                    {
                                            handler.Parameters[0].Name,
                                            handler.Compile()
                                            }
                            }
            };
        }
    }
}