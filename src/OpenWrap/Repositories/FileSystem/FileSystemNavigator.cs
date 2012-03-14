using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using OpenFileSystem.IO;
using OpenWrap.PackageManagement.Packages;
using OpenWrap.PackageModel;
using OpenWrap.Repositories.Http;


namespace OpenWrap.Repositories.FileSystem
{
    // TODO: Add locking support
    public class FileSystemNavigator : IHttpRepositoryNavigator
    {
        readonly IDirectory _directory;
        readonly IFile _indexFile;

        XDocument _indexDocument;

        public FileSystemNavigator(IDirectory directory)
        {
            _directory = directory;
            _indexFile = _directory.GetFile("index.wraplist");
        }

        public bool CanPublish
        {
            get { return true; }
        }

        public XDocument IndexDocument
        {
            get
            {
                EnsureIndexFileLoaded();
                return _indexDocument;
            }
            private set { _indexDocument = value; }
        }


        public PackageFeed Index()
        {
            var feed = IndexDocument.ParsePackageDocument();
            feed.CanPublish = true;
            return feed;
        }

        public Stream LoadPackage(PackageEntry packageEntry)
        {
            var packageFile = _directory.GetFile(packageEntry.PackageHref.ToString());
            return packageFile.Exists ? packageFile.OpenRead() : null;
        }

        public void PushPackage(string packageFileName, Stream packageStream)
        {
            packageFileName = PackageNameUtility.NormalizeFileName(packageFileName);

            var packageFile = _directory.GetFile(packageFileName);
            using (var destinationStream = packageFile.OpenWrite())
                packageStream.CopyTo(destinationStream);

#pragma warning disable 612,618
            var zipPackage = new ZipFilePackage(packageFile);
            IndexDocument.Document.Root.Add(
                    new XElement("wrap",
                                 new XAttribute("name", zipPackage.Name),
                                 new XAttribute("version", zipPackage.Version),
                                 new XAttribute("semantic-version", zipPackage.SemanticVersion),
                                 new XElement("link",
                                              new XAttribute("rel", "package"),
                                              new XAttribute("href", packageFile.Name)),
                                 zipPackage.Dependencies.Select(x => new XElement("depends", x.ToString()))
                            ));
#pragma warning restore 612,618

            SaveIndex(IndexDocument);

            return;
        }

        internal void Nuke(IPackageInfo packageInfo)
        {
            if (packageInfo.Nuked)
                throw new InvalidOperationException("The package has already been nuked.");

#pragma warning disable 612,618
            var packageVersionNode = (from node in IndexDocument.Descendants("wrap")
                                      let versionAttribute = node.Attribute("version")
                                      let semverAttribute = node.Attribute("semantic-version")
                                      let nameAttribute = node.Attribute("name")
                                      where nameAttribute != null && nameAttribute.Value.EqualsNoCase(packageInfo.Name) &&
                                            ((semverAttribute != null && semverAttribute.Value.Equals(packageInfo.SemanticVersion.ToString())) ||
                                             (versionAttribute != null && versionAttribute.Value.Equals(packageInfo.Version.ToString())))
                                      select node).FirstOrDefault();
#pragma warning restore 612,618

            if (packageVersionNode == null)
                throw new InvalidOperationException(
                        string.Format("The package {0} {1} does not exist in the index.",
                                      packageInfo.Name,
                                      packageInfo.SemanticVersion));

            packageVersionNode.Add(new XAttribute("nuked", true));

            SaveIndex(IndexDocument);
        }

        void EnsureIndexFileLoaded()
        {
            if (_indexDocument != null)
                return;
            _directory.MustExist();
            if (!_indexFile.Exists)
            {
                SaveIndex(XDocument.Parse("<wraplist/>"));
            }


            using (var stream = _indexFile.OpenRead())
                IndexDocument = XDocument.Load(new XmlTextReader(stream));
        }

        void SaveIndex(XDocument xDocument)
        {
            using (var stream = _indexFile.OpenWrite())
            {
                xDocument.Save(new StreamWriter(stream, Encoding.UTF8));
            }
        }
    }
}