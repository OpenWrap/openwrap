using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using OpenFileSystem.IO;
using OpenWrap.Dependencies;

namespace OpenWrap.Repositories
{
    public class IndexedFolderRepository : IPackageRepository
    {
        readonly IDirectory _directory;
        IFile _indexFile;

        public IndexedFolderRepository(IDirectory directory)
        {
            _directory = directory.MustExist();
            _indexFile = _directory.GetFile("index.wraplist");

            LoadOrCreateIndexFile();
        }

        void LoadOrCreateIndexFile()
        {
            if (!_indexFile.Exists)
            {
                SaveIndex(XDocument.Parse("<wraplist/>"));
            }


            using(var stream = _indexFile.OpenRead())
                IndexDocument = XDocument.Load(new XmlTextReader(stream));
        }

        void SaveIndex(XDocument xDocument)
        {
            using(var stream = _indexFile.OpenWrite())
            {
                xDocument.Save(new StreamWriter(stream, Encoding.UTF8));
            }
        }

        public ILookup<string, IPackageInfo> PackagesByName
        {
            get { throw new NotImplementedException(); }
        }

        public IPackageInfo Find(WrapDependency dependency)
        {
            throw new NotImplementedException();
        }

        public IPackageInfo Publish(string packageFileName, Stream packageStream)
        {
            var packageFile = _directory.GetFile(packageFileName);
            using (var destinationStream = packageFile.OpenWrite())
                packageStream.CopyTo(destinationStream);

            var zipPackage = new CachedZipPackage(this, packageFile, null, ExportBuilders.All, false);
            IndexDocument.Document.Root.Add(
                new XElement("wrap",
                    new XAttribute("name", zipPackage.Name),
                    new XAttribute("version", zipPackage.Version),
                    new XElement("link",
                        new XAttribute("rel", "package"),
                        new XAttribute("href", packageFile.Name))
                    ));

            SaveIndex(IndexDocument);
            
            return zipPackage;
        }

        public bool CanPublish
        {
            get { throw new NotImplementedException(); }
        }

        public string Name
        {
            get { throw new NotImplementedException(); }
        }

        public XDocument IndexDocument { get; private set; }
    }
}