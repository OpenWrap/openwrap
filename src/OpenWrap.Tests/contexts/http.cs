using System;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using OpenRasta.Client;
using OpenRasta.Client.Memory;
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
}