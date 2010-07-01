using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using OpenWrap.Exports;
using OpenWrap.Dependencies;
using OpenWrap.IO;
using OpenWrap.Repositories;

namespace OpenWrap.Repositories
{
    public class XmlRepository : IPackageRepository
    {
        readonly IHttpNavigator _navigator;

        public XmlRepository(IFileSystem fileSystem, IHttpNavigator navigator, IEnumerable<IExportBuilder> builders)
        {
            _navigator = navigator;
            var document = navigator.LoadFileList();

            PackagesByName = (from wrapList in document.Descendants("wrap")
                              let name = wrapList.Attribute("name")
                              let version = wrapList.Attribute("version")
                              let lastModifiedTimeUtc = GetModifiedTimeUtc(wrapList.Attribute("last-modified-time-utc"))
                              let link = (from link in wrapList.Elements("link")
                                          let relAttribute = link.Attribute("rel")
                                          let hrefAttribute = link.Attribute("href")
                                          where hrefAttribute != null && relAttribute != null && relAttribute.Value.Equals("package", StringComparison.OrdinalIgnoreCase)
                                          select hrefAttribute).FirstOrDefault()
                              let baseUri = !string.IsNullOrEmpty(document.BaseUri) ? new Uri(document.BaseUri, UriKind.Absolute) : null
                              let absoluteLink = baseUri == null ? new Uri(link.Value, UriKind.RelativeOrAbsolute) : new Uri(baseUri, new Uri(link.Value, UriKind.RelativeOrAbsolute))
                              where name != null && version != null && link != null
                              let depends = wrapList.Elements("depends").Select(x => x.Value)
                              select new XmlPackageInfo(fileSystem, this, navigator, name.Value, version.Value, absoluteLink, depends, builders, lastModifiedTimeUtc))
                .Cast<IPackageInfo>().ToLookup(x => x.Name);
        }

        DateTime? GetModifiedTimeUtc(XAttribute attribute)
        {
            if (attribute == null) return null;
            DateTime dt;
            return !DateTime.TryParse(attribute.Value, out dt) ? (DateTime?)null : dt;
        }

        public ILookup<string, IPackageInfo> PackagesByName { get; private set; }

        public IPackageInfo Find(WrapDependency dependency)
        {
            return PackagesByName.Find(dependency);
        }

        public IPackageInfo Publish(string packageFileName, Stream packageStream)
        {
            throw new NotImplementedException();
        }

        public bool CanPublish
        {
            get { return false; }
        }

        public string Name
        {
            get { return string.Format("Remote [{0}]", _navigator); }
        }
    }
}