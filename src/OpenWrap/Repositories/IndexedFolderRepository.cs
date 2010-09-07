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

        public IndexedFolderRepository(IDirectory directory)
        {
            _directory = directory.MustExist();
            LoadOrCreateIndexFile();
        }

        void LoadOrCreateIndexFile()
        {
            var indexFile = _directory.GetFile("index.wraplist");
            if (!indexFile.Exists)
            {
                using(var stream = indexFile.OpenWrite())
                    XDocument.Parse("<wraplist/>").Save(new StreamWriter(stream, Encoding.UTF8));
            }


            using(var stream = indexFile.OpenRead())
                IndexFile = XDocument.Load(new XmlTextReader(stream));
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

            return new ZipPackage(this, packageFile, null, ExportBuilders.All, false);
        }

        public bool CanPublish
        {
            get { throw new NotImplementedException(); }
        }

        public string Name
        {
            get { throw new NotImplementedException(); }
        }

        public XDocument IndexFile { get; private set; }
    }
}