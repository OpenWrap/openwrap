using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using OpenRasta.Client;
using OpenWrap;
using OpenWrap.PackageModel;
using OpenWrap.Repositories.Http;
using OpenWrap.Repositories.NuGet;

namespace OpenWrap.Repositories.NuFeed
{
    public class NuFeedReader
    {
        const string NS_ATOM = "http://www.w3.org/2005/Atom";
        const string NS_ATOMPUB = "http://www.w3.org/2007/app";
        const string NS_XML = "http://www.w3.org/XML/1998/namespace";
        public static PackageFeed Read(XmlReader reader)
        {
            var feed = XDocument.Load(reader).Feed();
            return new PackageFeed
            {
                    BaseUri = feed.AttrValue("base", NS_XML).ToUri(),
                    CanPublish = false,
                    Packages = from entryElement in feed.Entries()
                               select entryElement.ToPackageEntry(),
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

    }
    public static class XDocumentExtensions
    {
        const string NS_XML = "http://www.w3.org/XML/1998/namespace";

        const string NS_ATOM = "http://www.w3.org/2005/Atom";
        const string NS_ODATA = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        const string NS_ODATA_META = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
        public static XElement Feed(this XContainer container)
        {
            return container.AtomElement("feed");
        }
        public static IEnumerable<XElement> Entries(this XContainer container)
        {
            return container.AtomElements("entry");
        }
        public static IEnumerable<XElement> AtomElements(this XContainer container, string name)
        {
            return container.Elements(XName.Get(name, NS_ATOM));
        }
        public static XElement AtomElement(this XContainer container, string name)
        {
            return container.Element(XName.Get(name, NS_ATOM));
        }
        public static XElement ODataProperties(this XContainer container)
        {
            return container.Element(XName.Get("properties", NS_ODATA_META));
        }
        //public static ILookup<string,XElement> AtomLinks(this XContainer container)
        //{
        //    return container.
        //}
        public static string ODataValue(this XContainer container, string name)
        {
            return container.Element(XName.Get(name, NS_ODATA)).Value();
        }
        public static string AttrValue(this XElement element, string attributeName, string ns)
        {
            var attr = element.Attribute(XName.Get(attributeName, ns));
            if (attr != null) return attr.Value;
            return null;
        }
        public static string AttrValue(this XElement element, string attributeName)
        {
            var attr = element.Attribute(attributeName);
            if (attr != null) return attr.Value;
            return null;
        }
        public static string Value(this XElement element)
        {
            return element == null ? null : element.Value;
        }

        public static PackageEntry ToPackageEntry(this XElement element)
        {
            var properties = element.ODataProperties();
            return new PackageEntry
            {
                    Name = properties.ODataValue("Id"),
                    Version = SemanticVersion.TryParseExact(properties.ODataValue("Version")),
                    Description = properties.ODataValue("Summary"),
                    Dependencies = ParseNugetDependencies(properties.ODataValue("Dependencies")),
                    PackageHref = (from content in element.Descendants(XName.Get("content", NS_ATOM))
                                   let type = content.Attribute("type")
                                   where type != null && type.Value == "application/zip"
                                   select content.AttributeAsUri("src")).FirstOrDefault()
            };
        }
        public static Uri AsUri(this XElement element)
        {
            var baseUris = (
                                                        from parent in element.AncestorsAndSelf()
                                                        let xmlBase = parent.Attribute(XName.Get("base", NS_XML))
                                                        where xmlBase != null
                                                        let parsedBaseUri = xmlBase.Value.ToUri()
                                                        select parsedBaseUri
                                                ).Reverse();
            if (baseUris.Count() == 0) return element.Value.ToUri();
            if (baseUris.Count() == 1) return baseUris.First().Combine(element.Value);
            return baseUris.Skip(1).Aggregate(baseUris.First(), UriExtensions.Combine).Combine(element.Value);
            }
        public static Uri AttributeAsUri(this XElement element, string attributeName)
        {
            var attribute = element.Attribute(attributeName);
            if (attribute == null) return null;
            var baseUris = (
                                                        from parent in element.AncestorsAndSelf()
                                                        let xmlBase = parent.Attribute(XName.Get("base", NS_XML))
                                                        where xmlBase != null
                                                        let parsedBaseUri = xmlBase.Value.ToUri()
                                                        select parsedBaseUri
                                                ).Reverse();

            if (baseUris.Count() == 0) return attribute.Value.ToUri();
            if (baseUris.Count() == 1) return baseUris.First().Combine(attribute.Value);
            return baseUris.Skip(1).Aggregate(baseUris.First(), UriExtensions.Combine).Combine(attribute.Value);
        }
        static IEnumerable<string> ParseNugetDependencies(string oDataValue)
        {
            if (oDataValue == null || ((oDataValue = oDataValue.Trim()) == string.Empty)) return Enumerable.Empty<string>();

            return from dependency in oDataValue.Split('|')
                   let splits = dependency.Split(':')
                   where splits.Length >= 1
                   let dep = new PackageDependencyBuilder(splits[0]).SetVersionVertices(NuConverter.ConvertNuGetVersionRange(splits.Length > 1 ? splits[1] : ""))
                   select dep.ToString();
        }
    }
}