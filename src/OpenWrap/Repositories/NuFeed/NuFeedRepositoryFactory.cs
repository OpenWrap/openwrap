using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using OpenFileSystem.IO;
using OpenRasta.Client;
using OpenWrap.Repositories.Http;
using OpenWrap.Repositories.NuGet;

namespace OpenWrap.Repositories.NuFeed
{
    public class NuFeedRepositoryFactory : IRemoteRepositoryFactory
    {
        const string DEFAULT_HREF = "https://go.microsoft.com/fwlink/?LinkID=206669";
        const string NS_ATOM = "http://www.w3.org/2005/Atom";
        const string NS_ATOMPUB = "http://www.w3.org/2007/app";
        const string NS_XML = "http://www.w3.org/XML/1998/namespace";
        readonly IFileSystem _fileSystem;
        readonly IHttpClient _client;

        public NuFeedRepositoryFactory(IFileSystem fileSystem, IHttpClient client)
        {
            _fileSystem = fileSystem;
            _client = client;
        }

        public IPackageRepository FromToken(string token)
        {
            if (token.StartsWith("[nuget]") == false) return null;
            var destinationUri = Regex.Match(token, @"^\[nuget\]\[(?<target>.*)\](?<uri>.*)");
            return new NuFeedRepository(_fileSystem, _client, destinationUri.Groups["target"].Value.ToUri(), destinationUri.Groups["uri"].Value.ToUri());
        }

        public IPackageRepository FromUserInput(string userInput, NetworkCredential credentails = null)
        {
            userInput = userInput.Trim();
            if (userInput.EqualsNoCase("nuget"))
                return TryLocate(DEFAULT_HREF);
            if (!userInput.StartsWithNoCase("http://") && !userInput.StartsWith("https://"))
                return TryLocate("https://" + userInput) ?? TryLocate("http://" + userInput);

            return TryLocate(userInput);
        }

        IPackageRepository TryLocate(string uri)
        {
            var target = uri.ToUri();
            if (target == null) return null;

            var serviceResponse = _client.CreateRequest(target).Get().Send();
            if (serviceResponse.Status.Code != 200) return null;
            var serviceDocument = XDocument.Load(XmlReader.Create(serviceResponse.Entity.Stream, new XmlReaderSettings { }), LoadOptions.SetBaseUri)
                    ;
            var packagesUri = (from collectionElement in serviceDocument.Descendants(XName.Get("collection", NS_ATOMPUB))
                              let collectionHref = collectionElement.Attributes("href").FirstOrDefault()
                              let titleElement = collectionElement.Element(XName.Get("title", NS_ATOM))
                              where collectionHref != null &&
                                    titleElement != null &&
                                    titleElement.Value == "Packages"
                              let baseElement = (
                                                        from parent in collectionElement.AncestorsAndSelf()
                                                        let xmlBase = parent.Attribute(XName.Get("base", NS_XML))
                                                        where xmlBase != null
                                                        let parsedBaseUri = xmlBase.Value.ToUri()
                                                        select parsedBaseUri
                                                ).Reverse().Aggregate(target, UriExtensions.Combine)
                              select baseElement.Combine(collectionHref.Value.ToUri())).FirstOrDefault();

            if (packagesUri == null) return null;
            return new NuFeedRepository(_fileSystem, _client, target, packagesUri);
        }
    }
}