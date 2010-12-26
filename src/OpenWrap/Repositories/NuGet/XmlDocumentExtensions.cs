using System.Collections.Generic;
using System.Linq;
using System.Xml;
using OpenWrap.Collections;

namespace OpenWrap.Repositories.NuGet
{
    public static class XmlDocumentExtensions
    {
        public static string Element(this XmlDocument document, string[] xpaths, XmlNamespaceManager ns)
        {
            return xpaths.Select(x => document.SelectSingleNode(x, ns)).NotNull().Select(x => x.InnerText).FirstOrDefault();
        }

        public static IEnumerable<XmlNode> Elements(this XmlDocument document, string[] xpaths, XmlNamespaceManager ns)
        {
            return xpaths.SelectMany(x => document.SelectNodes(x, ns).OfType<XmlNode>());
        }
    }
}