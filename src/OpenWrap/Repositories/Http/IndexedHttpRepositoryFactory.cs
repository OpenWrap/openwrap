using System;
using System.Linq;
using System.Net;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenRasta.Client;

namespace OpenWrap.Repositories.Http
{
    public class IndexedHttpRepositoryFactory : IRemoteRepositoryFactory
    {
        const string PREFIX = "[indexed-http]";
        const string PREFIX_LEGACY = "[indexed]";
        readonly IHttpClient _client;

        public IndexedHttpRepositoryFactory(IHttpClient client)
        {
            _client = client;
        }

        public IPackageRepository FromToken(string token)
        {
            // legacy prefix was used by mistake in 2.0.0, fixed in 2.0.1 but 
            // need parsing, which we do in 2.0.2
            if (token.StartsWith(PREFIX_LEGACY))
                token = PREFIX + token.Substring(PREFIX_LEGACY.Length).ToUri().Combine("index.wraplist");
            if (token.StartsWith(PREFIX) == false)
                return null;
            return GetIndexedRepository(token.Substring(PREFIX.Length).ToUri());
        }

        public IPackageRepository FromUserInput(string userInput, NetworkCredential credentials = null)
        {
            bool found = false;
            var targetUri = userInput.ToUri();
            if (targetUri == null ||
                targetUri.IsAbsoluteUri == false ||
                !(targetUri.Scheme.EqualsNoCase("http") || targetUri.Scheme.EqualsNoCase("https")))
                return null;

            if (!targetUri.Segments.Last().EqualsNoCase("index.wraplist"))
                targetUri = targetUri.EnsureTrailingSlash().Combine("index.wraplist");
            _client.Head(targetUri)
                                 .Credentials(credentials)
                                 .Handle(200, _ => found = true)
                                 .Send();
            var repository = GetIndexedRepository(targetUri);
            if (credentials != null)
                repository = new PreAuthenticatedRepository(repository, repository.Feature<ISupportAuthentication>(), credentials);
            return found ? repository : null;
        }

        IPackageRepository GetIndexedRepository(Uri targetUri)
        {
            // TODO: Remove the inmemory file system
            // TODO: Rename repository
            return new IndexedHttpRepository(new InMemoryFileSystem(), "local", new HttpRepositoryNavigator(_client, targetUri))
            {
                Token = PREFIX + targetUri
            };
        }
    }
}