using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using OpenFileSystem.IO;
using OpenRasta.Client;
using OpenWrap.Repositories.Http;
using OpenWrap.Repositories.NuGet;

namespace OpenWrap.Repositories
{
    public class NuGetODataRepositoryFactory : IRemoteRepositoryFactory
    {
        const string DEFAULT_HREF = "https://go.microsoft.com/fwlink/?LinkID=206669";
        const string NS_ATOM = "http://www.w3.org/2005/Atom";
        const string NS_ATOMPUB = "http://www.w3.org/2007/app";
        const string NS_XML = "http://www.w3.org/XML/1998/namespace";
        readonly IHttpClient _client;

        public NuGetODataRepositoryFactory(IFileSystem fileSystem, IHttpClient client)
        {
            _client = client;
        }

        public IPackageRepository FromToken(string token)
        {
            if (token.StartsWith("[nuget]") == false) return null;
            var destinationUri = Regex.Match(token, @"^\[nuget\]\[.*\](?<uri>.*)");
            return new HttpRepository(
                    null,
                    "nuget",
                    new NuGetFeedNavigator(destinationUri.Groups["uri"].Value.ToUri()))
            {
                Token = token
            }; 
            
        }

        public IPackageRepository FromUserInput(string identifier)
        {
            identifier = identifier.Trim();
            if (identifier.EqualsNoCase("nuget"))
                return TryLocate(DEFAULT_HREF);
            if (!identifier.StartsWithNoCase("http://") || !identifier.StartsWith("https://"))
                return TryLocate("https://" + identifier) ?? TryLocate("http://" + identifier);

            return TryLocate(identifier);
        }

        IPackageRepository TryLocate(string uri)
        {
            var target = uri.ToUri();
            if (target == null) return null;

            var serviceResponse = _client.CreateRequest(target).Get().Send();
            if (serviceResponse.Status.Code != 200) return null;
            var serviceDocument = XDocument.Load(XmlReader.Create(serviceResponse.Entity.Stream, new XmlReaderSettings { }), LoadOptions.SetBaseUri)
                    ;
            var packageUri = (from collection in serviceDocument.Descendants(XName.Get("collection", NS_ATOMPUB))
                              let collectionHref = collection.Attributes("href").FirstOrDefault()
                              let titleElement = collection.Element(XName.Get("title", NS_ATOM))
                              where collectionHref != null &&
                                    titleElement != null &&
                                    titleElement.Value == "Packages"
                              let baseElement = (
                                                        from parent in collection.AncestorsAndSelf()
                                                        let xmlBase = parent.Attribute(XName.Get("base", NS_XML))
                                                        where xmlBase != null
                                                        let parsedBaseUri = xmlBase.Value.ToUri()
                                                        select parsedBaseUri
                                                ).Reverse().Aggregate(target, UriExtensions.Combine)
                              select baseElement.Combine(collectionHref.Value.ToUri())).FirstOrDefault();

            if (packageUri == null) return null;
            return new HttpRepository(
                    null,
                    "nuget",
                    new NuGetFeedNavigator(packageUri))
            {
                    Token = string.Format("[nuget][{0}]{1}", target, packageUri)
            };
        }
    }
}