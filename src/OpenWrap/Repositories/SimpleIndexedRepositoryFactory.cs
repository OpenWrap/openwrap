using System;
using System.Linq;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenRasta.Client;
using OpenWrap.Repositories.Http;

namespace OpenWrap.Repositories
{
    public class SimpleIndexedRepositoryFactory : IRemoteRepositoryFactory
    {
        IHttpClient _client;

        public SimpleIndexedRepositoryFactory(IHttpClient client)
        {
            _client = client;
        }
        
        public IPackageRepository FromToken(string token)
        {
            if (token.StartsWith("[indexed]") == false)
                return null;
            return GetIndexedRepository(token.Substring(9).ToUri());
        }

        public IPackageRepository FromUserInput(string identifier)
        {
            // try a HEAD on /index.wraplist to see if it is the correct one
            bool found = false;
            var targetUri = identifier.ToUri();
            if (targetUri == null) return null;

            if (!StringExtensions.EqualsNoCase(targetUri.Segments.Last(), "index.wraplist"))
                targetUri = new Uri(targetUri.EnsureTrailingSlash(), new Uri("index.wraplist", UriKind.Relative));
            _client.Head(targetUri).Handle(200, _ => found = true).Send();
            return found ? GetIndexedRepository(targetUri) : null;
        }

        IPackageRepository GetIndexedRepository(Uri targetUri)
        {
            return new HttpRepository(new InMemoryFileSystem(), "local", new HttpRepositoryNavigator(targetUri))
            {
                    Token = "[indexed]" + targetUri
            };
        }
    }
}