using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Xml;
using OpenWrap.Repositories.Http;

namespace OpenWrap.Repositories.NuGet
{
    public class NuGetSyndicationItem : SyndicationItem
    {
        IEnumerable<NuGetDependency> _oDataDependencies;
        bool? _oDataFound;

        string _oDataPackageVersion;
        string _oDataPublished;
        string _odataId;
        string _odataAuthors;
        string _odataSummary;

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

        public string PackageDescription
        {
            get
            {
                TextSyndicationContent content = null;
                return ODataNode()
                    ? _odataSummary ?? (Summary != null ? Summary.Text : string.Empty)
                               : ((content = Content as TextSyndicationContent) != null)
                                         ? content.Text
                                         : String.Empty;
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

        public string PackageName
        {
            get
            {
                if (ODataNode())
                    return _odataId ?? (Title == null ? string.Empty : Title.Text);
                return ElementExtensions.Extension<string>("packageId");
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

        public string PackageVersion
        {
            get
            {
                if (ODataNode())
                    return _oDataPackageVersion;
                return ElementExtensions.Extension<string>("version");
            }
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

        NuGetDependency[] GetDependencies()
        {
            return ElementExtensions.OptionalExtension<NuGetDependency[]>("dependencies");
        }

        IEnumerable<NuGetDependency> GetODataDependencies(string dependencyString)
        {
            return (from dependency in dependencyString.Split(new[] { ',', '|' }, StringSplitOptions.RemoveEmptyEntries)
                    let chunks = dependency.Split(':')
                    select new NuGetDependency
                    {
                            Id = chunks[0],
                            MinVersion = chunks.Length == 4 ? chunks[1] : null,
                            MaxVersion = chunks.Length == 4 ? chunks[2] : null,
                            ExactVersion = chunks.Length == 4 ? chunks[3] : null,
                            Version = chunks.Length == 2 ? chunks[1] : null
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

                using (var reader = extension.GetReader())
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
                        else if (reader.LocalName == "Id")
                            _odataId = reader.ReadElementContentAsString();
                        else if (reader.LocalName == "Version")
                            _oDataPackageVersion = reader.ReadElementContentAsString();
                        else if (reader.LocalName == "Authors")
                            _odataAuthors = reader.ReadElementContentAsString();
                        else if (reader.LocalName == "Summary")
                            _odataSummary = reader.ReadElementContentAsString();
                    }
                }
            }
            return true;
        }
    }
}