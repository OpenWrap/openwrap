using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;
using OpenWrap.Repositories.Http;

namespace OpenWrap.Repositories.NuGet
{
    public class NuGetSyndicationItem : SyndicationItem
    {
        XDocument _oDataNode;
        
        string _oDataPackageVersion;
        IEnumerable<NuGetDependency> _oDataDependencies;
        bool? _oDataFound;
        string _oDataPublished;

        public string PackageName
        {
            get
            {
                if (ODataNode())
                    return Title.Text;
                return ElementExtensions.Extension<string>("packageId");
            }
        }
        public string PackageVersion
        {
            get
            {
                if (ODataNode())
                    return _oDataPackageVersion;
                return ElementExtensions.Extension<string>("version");
            }
        }
        public string PackageDescription
        {
            get
            {
                TextSyndicationContent content = null;
                return ODataNode()
                               ? Summary.Text
                               : ((content = Content as TextSyndicationContent) != null)
                                         ? content.Text
                                         : null;
            }
        }
        public string PackagePublished
        {
            get
            {
                if (ODataNode()) return _oDataPublished;
                return new DateTimeOffset(PublishDate.UtcDateTime).ToString();
            }
        }
        public Uri PackageHref
        {
            get
            {
                ODataNode();

                var url = this.Content as UrlSyndicationContent;
                if (url != null && url.Type == "application/zip")
                    return url.Url;

                return Links.Where(x => x.RelationshipType.EqualsNoCase("enclosure")).First().GetAbsoluteUri();
            }
        }
        public List<string> Dependencies
        {
            get
            {
                ODataNode();
               
                var deps = _oDataDependencies 
                           ?? GetDependencies()
                           ?? Enumerable.Empty<NuGetDependency>();

                return deps.Select(x => x.ToPackageDependencyLine()).ToList();
            }
        }

        IEnumerable<NuGetDependency> GetODataDependencies(string dependencyString)
        {
            return (from dependency in dependencyString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                   let chunks = dependency.Split(':')
                   select new NuGetDependency
                   {
                           Id = chunks[0],
                           MinVersion = chunks[1],
                           MaxVersion = chunks[2],
                           Version = chunks[3]
                   }).ToList();
        }

        bool ODataNode()
        {
            if (_oDataFound != null)
                return _oDataFound.Value;
            if (_oDataFound == null)
            {
                var extension = ElementExtensions.FirstOrDefault(x => x.OuterName == "properties" && x.OuterNamespace == Namespaces.AstoriaM);
                
                if ((_oDataFound = (extension != null)) == false) return false;

                using(var reader = extension.GetReader())
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType != XmlNodeType.Element) continue;
                        if (reader.LocalName == "Version")
                            _oDataPackageVersion = reader.ReadElementContentAsString();
                        else if (reader.LocalName == "Dependencies")
                            _oDataDependencies = GetODataDependencies(reader.ReadElementContentAsString());
                        else if (reader.LocalName == "Published")
                            _oDataPublished = reader.ReadElementContentAsString();
                    }
                }
            }
            return true;
        }

        NuGetDependency[] GetDependencies()
        {
            return ElementExtensions.OptionalExtension<NuGetDependency[]>("dependencies");
        }

        public PackageItem ToPackage()
        {
            return new PackageItem
            {
                Dependencies = Dependencies,
                Name = PackageName,
                Version = PackageVersion.ToVersion(),
                Description = PackageDescription,
                PackageHref = PackageHref,
                CreationTime = PackagePublished == null ? default(DateTimeOffset) : DateTimeOffset.Parse(PackagePublished)
            };
        }
    }
}