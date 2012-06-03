using System;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using OpenRasta.Client;
using OpenWrap.PackageModel;
using OpenWrap.Repositories.Http;

namespace OpenWrap.Repositories.NuFeed
{
    public class NuFeedReader
    {
        const string NS_XML = "http://www.w3.org/XML/1998/namespace";

        public static PackageFeed Read(XmlReader reader)
        {
            var xDocument = XDocument.Load(reader, LoadOptions.SetBaseUri);

            var feed = xDocument.Feed();
            
            return new PackageFeed
            {
                BaseUri = (feed.AttrValue("base", NS_XML) ?? xDocument.BaseUri).ToUri(),
                CanPublish = false,
                Packages = from entryElement in feed.Entries()
                           let entry = entryElement.ToPackageEntry()
                           where EntryIsValid(entry)
                           select entry,
                LastUpdate = TryParseDate(feed.AtomElement("updated").Value()),
                Links = (
                            from link in feed.AtomElements("link")
                            select new AtomLink
                            {
                                Rel = link.AttrValue("rel"),
                                Href = link.AttrValue("href")
                            }
                        ).ToLookup(x => x.Rel)
            };
        }

        static bool EntryIsValid(PackageEntry entry)
        {
            var entryIsValid = entry.Name != null &&
                               entry.Version != null &&
                               PackageNameUtility.IsNameValid(entry.Name) &&
                               entry.Dependencies.All(_ => PackageNameUtility.IsNameValid(_.Split(' ')[0]));
            if (!entryIsValid)
                Debug.WriteLine("invalid package");
            return entryIsValid;
        }

        static DateTimeOffset? TryParseDate(string value)
        {
            if (value == null) return null;
            DateTimeOffset returnValue;
            return DateTimeOffset.TryParse(value, out returnValue) ? returnValue : (DateTimeOffset?)null;
        }
    }
}