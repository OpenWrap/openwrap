using System;
using System.Linq;
using System.Xml.Linq;
using OpenRasta.Client;
using OpenWrap.Tasks;

namespace OpenWrap.Repositories.Http
{
    public static class RequestExtensions
    {
        public static PackageFeed AsPackageDocument<T>(this T response) where T : IClientResponse
        {
            if (response == null)
                return null;
            return response.AsXDocument().ParsePackageDocument();
        }

        public static T Notify<T>(this T request, ITaskChanges task) where T : IProgressNotification
        {
            request.StatusChanged += (s, e) => task.Status(e.Message);
            request.Progress += (s, e) => task.Progress(e.Progress);
            return request;
        }

        public static PackageFeed ParsePackageDocument(this XDocument xmlDocument)
        {
            if (xmlDocument == null)
                return null;
            var feed = new PackageFeed
            {
                PublishHref = GetPublishHref(xmlDocument),
                Packages = from wrapList in xmlDocument.Descendants("wrap")
                           let name = wrapList.Attribute("name")
                           let version = wrapList.Attribute("version")
                           let nuked = wrapList.Attribute("nuked")
                           let lastModifiedTimeUtc = GetModifiedTimeUtc(wrapList.Attribute("last-modified-time-utc"))
                           let link = (from link in wrapList.Elements("link")
                                       let relAttribute = link.Attribute("rel")
                                       let hrefAttribute = link.Attribute("href")
                                       where hrefAttribute != null && relAttribute != null && relAttribute.Value.EqualsNoCase("package")
                                       select hrefAttribute).FirstOrDefault()
                           let baseUri = !string.IsNullOrEmpty(xmlDocument.BaseUri) ? new Uri(xmlDocument.BaseUri, UriKind.Absolute) : null
                           let absoluteLink = baseUri == null ? new Uri(link.Value, UriKind.RelativeOrAbsolute) : new Uri(baseUri, new Uri(link.Value, UriKind.RelativeOrAbsolute))
                           where name != null && version != null && link != null
                           let depends = wrapList.Elements("depends").Select(x => x.Value)
                           select new PackageEntry
                           {
                               Name = name.Value,
                               Version = SemanticVersion.TryParseExact(version.Value),
                               PackageHref = absoluteLink,
                               Dependencies = depends,
                               CreationTime = lastModifiedTimeUtc,
                               Nuked = nuked == null ? false : GetNuked(nuked.Value)
                           }
            };
            feed.CanPublish = feed.PublishHref != null;
            return feed;
        }

        static DateTimeOffset GetModifiedTimeUtc(XAttribute attribute)
        {
            var now = DateTimeOffset.UtcNow;
            if (attribute == null) return now;
            DateTimeOffset dt;
            return !DateTimeOffset.TryParse(attribute.Value, out dt) ? now : dt;
        }

        static bool GetNuked(string s)
        {
            bool b;
            if (Boolean.TryParse(s, out b))
                return b;
            else
                return false;
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
    }
}