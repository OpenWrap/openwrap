using System;
using System.Linq;
using System.Xml.Linq;
using OpenRasta.Client;
using OpenWrap.Tasks;

namespace OpenWrap.Repositories.Http
{
    public static class RequestExtensions
    {
        public static T Notify<T>(this T request, ITaskChanges task) where T : IProgressNotification
        {
            request.StatusChanged += (s, e) => task.Status(e.Message);
            request.Progress += (s, e) => task.Progress(e.Progress);
            return request;
        }
        public static PackageDocument AsPackageDocument<T>(this T response) where T : IClientResponse
        {
            if (response == null)
                return null;
            return response.AsXDocument().ParsePackageDocument();
        }
        public static PackageDocument ParsePackageDocument(this XDocument xmlDocument)
        {
            if (xmlDocument == null)
                return null;
            return new PackageDocument
            {
                    CanPublish = GetCanPublish(xmlDocument),
                    PublishHref = GetPublishHref(xmlDocument),
                    Packages = from wrapList in xmlDocument.Descendants("wrap")
                               let name = wrapList.Attribute("name")
                               let version = wrapList.Attribute("version")
                               let lastModifiedTimeUtc = GetModifiedTimeUtc(wrapList.Attribute("last-modified-time-utc"))
                               let link = (from link in wrapList.Elements("link")
                                           let relAttribute = link.Attribute("rel")
                                           let hrefAttribute = link.Attribute("href")
                                           where hrefAttribute != null && relAttribute != null && relAttribute.Value.Equals("package", StringComparison.OrdinalIgnoreCase)
                                           select hrefAttribute).FirstOrDefault()
                               let baseUri = !string.IsNullOrEmpty(xmlDocument.BaseUri) ? new Uri(xmlDocument.BaseUri, UriKind.Absolute) : null
                               let absoluteLink = baseUri == null ? new Uri(link.Value, UriKind.RelativeOrAbsolute) : new Uri(baseUri, new Uri(link.Value, UriKind.RelativeOrAbsolute))
                               where name != null && version != null && link != null
                               let depends = wrapList.Elements("depends").Select(x => x.Value)
                               select new PackageItem
                               {
                                       Name = name.Value,
                                       Version = new Version(version.Value),
                                       PackageHref = absoluteLink,
                                       Dependencies = depends,
                                       CreationTime = lastModifiedTimeUtc
                               }
            };
        }

        static Uri GetPublishHref(XDocument xmlDocument)
        {
            return (from link in xmlDocument.Descendants("link")
                    let hrefAttribute = link.Attribute("href")
                    let relAttribute = link.Attribute("rel")
                    where hrefAttribute != null &&
                          relAttribute != null &&
                          relAttribute.Value == "publish"
                    let baseUri = new Uri(xmlDocument.BaseUri, UriKind.Absolute)
                    select new Uri(baseUri, hrefAttribute.Value))
                    .FirstOrDefault();

            
        }

        static bool GetCanPublish(XDocument doc)
        {
            var wraplist = doc.Element("wraplist");
            var canPublishNode = wraplist != null ? wraplist.Attribute("read-only") : null;
            bool readOnly;
            if (canPublishNode != null && bool.TryParse(canPublishNode.Value, out readOnly))
                return !readOnly;
            return false;
        }

        static DateTimeOffset GetModifiedTimeUtc(XAttribute attribute)
        {
            var now = DateTimeOffset.UtcNow;
            if (attribute == null) return now;
            DateTimeOffset dt;
            return !DateTimeOffset.TryParse(attribute.Value, out dt) ? now : dt;
        }
    }
}