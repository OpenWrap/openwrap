using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using OpenWrap.Repositories.Http;

namespace OpenWrap.Repositories
{
    public class HttpRepositoryNavigator : IHttpRepositoryNavigator
    {
        readonly Uri _serverUri;
        WebRequest _request;
        Uri _requestUri;
        XDocument _fileList;

        public HttpRepositoryNavigator(Uri serverUri)
        {
            _serverUri = serverUri;
            _requestUri = new Uri(serverUri, new Uri("index.wraplist", UriKind.Relative));
        }
        public PackageDocument Index()
        {
            EnsureFileListLoaded();
            XDocument file = _fileList;
            return ParseXDocument(file);
        }

        public static PackageDocument ParseXDocument(XDocument file)
        {
            return new PackageDocument
            {
                    Packages = from wrapList in file.Descendants("wrap")
                               let name = wrapList.Attribute("name")
                               let version = wrapList.Attribute("version")
                               let lastModifiedTimeUtc = GetModifiedTimeUtc(wrapList.Attribute("last-modified-time-utc"))
                               let link = (from link in wrapList.Elements("link")
                                           let relAttribute = link.Attribute("rel")
                                           let hrefAttribute = link.Attribute("href")
                                           where hrefAttribute != null && relAttribute != null && relAttribute.Value.Equals("package", StringComparison.OrdinalIgnoreCase)
                                           select hrefAttribute).FirstOrDefault()
                               let baseUri = !string.IsNullOrEmpty(file.BaseUri) ? new Uri(file.BaseUri, UriKind.Absolute) : null
                               let absoluteLink = baseUri == null ? new Uri(link.Value, UriKind.RelativeOrAbsolute) : new Uri(baseUri, new Uri(link.Value, UriKind.RelativeOrAbsolute))
                               where name != null && version != null && link != null
                               let depends = wrapList.Elements("depends").Select(x => x.Value)
                               select new PackageItem
                               {
                                       Name = name.Value,
                                       Version = new Version(version.Value),
                                       PackageHref = absoluteLink,
                                       Dependencies = depends,
                                       LastModifiedTimeUtc = lastModifiedTimeUtc
                               }
            };
        }

        void EnsureFileListLoaded()
        {
            if (_fileList == null)
            {
                _fileList = XDocument.Load(_requestUri.ToString(), LoadOptions.SetBaseUri);
            }
        }

        public Stream LoadPackage(PackageItem packageItem)
        {
            return WebRequest.Create(packageItem.PackageHref).GetResponse().GetResponseStream();
        }

        static DateTime? GetModifiedTimeUtc(XAttribute attribute)
        {
            if (attribute == null) return null;
            DateTime dt;
            return !DateTime.TryParse(attribute.Value, out dt) ? (DateTime?)null : dt;
        }
    }
}