using System.IO;
using System.Linq;
using OpenRasta.Wrap.Dependencies;
using OpenRasta.Wrap.Repositories;

namespace OpenRasta.Wrap.Sources
{
    public class FolderRepository : IWrapRepository
    {
        /// <exception cref="DirectoryNotFoundException"><c>DirectoryNotFoundException</c>.</exception>
        public FolderRepository(string basePath)
        {
            
            BasePath = basePath;
            if (!Directory.Exists(basePath))
                throw new DirectoryNotFoundException(string.Format("The directory '{0}' doesn't exist.", basePath));

            var baseDirectory = new DirectoryInfo(basePath);
            PackagesByName = (from directory in baseDirectory.GetDirectories()
                              let directoryName = directory.Name
                              let packageVersion = WrapNameUtility.GetVersion(directoryName)
                              where packageVersion != null
                              select (IWrapPackage)new FolderWrapPackage(directory.FullName, new[] { new AssemblyReferenceExportBuider() }))
                .ToLookup(x => x.Name);
        }

        public string BasePath { get; set; }

        public ILookup<string, IWrapPackage> PackagesByName { get; private set; }

        public IWrapPackage Find(WrapDependency dependency)
        {
            if (!PackagesByName.Contains(dependency.Name))
                return null;
            return (from package in PackagesByName[dependency.Name]
                    where package.Version != null && dependency.IsFulfilledBy(package.Version)
                    orderby package.Version descending
                    select package).FirstOrDefault();
        }
    }
}