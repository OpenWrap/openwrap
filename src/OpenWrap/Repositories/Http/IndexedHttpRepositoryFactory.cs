using System;
using System.Linq;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenRasta.Client;

namespace OpenWrap.Repositories.Http
{
    public class IndexedHttpRepositoryFactory : IRemoteRepositoryFactory
    {
        const string PREFIX = "[indexed-http]";
        readonly IHttpClient _client;

        public IndexedHttpRepositoryFactory(IHttpClient client)
        {
            _client = client;
        }

        public IPackageRepository FromToken(string token)
        {
            if (token.StartsWith(PREFIX) == false)
                return null;
            return GetIndexedRepository(token.Substring(PREFIX.Length).ToUri());
        }

        public IPackageRepository FromUserInput(string identifier)
        {
            // try a HEAD on /index.wraplist to see if it is the correct one
            bool found = false;
            var targetUri = identifier.ToUri();
            if (targetUri == null ||
                targetUri.IsAbsoluteUri == false ||
                !(targetUri.Scheme.EqualsNoCase("http") || targetUri.Scheme.EqualsNoCase("https")))
                return null;

            if (!targetUri.Segments.Last().EqualsNoCase("index.wraplist"))
                targetUri = new Uri(targetUri.EnsureTrailingSlash(), new Uri("index.wraplist", UriKind.Relative));
            _client.Head(targetUri).Handle(200, _ => found = true).Send();
            return found ? GetIndexedRepository(targetUri) : null;
        }

        IPackageRepository GetIndexedRepository(Uri targetUri)
        {
            // TODO: Remove the inmemory file system
            return new IndexedHttpRepository(new InMemoryFileSystem(), "local", new HttpRepositoryNavigator(_client, targetUri))
            {
                Token = PREFIX + targetUri
            };
        }
    }
}