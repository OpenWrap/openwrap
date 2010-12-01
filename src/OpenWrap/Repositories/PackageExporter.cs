using System.Collections.Generic;
using System.Linq;
using OpenWrap.Dependencies;
using OpenWrap.Exports;

namespace OpenWrap.Repositories
{
    public class PackageExporter : IPackageExporter
    {
        public IEnumerable<T> GetExports<T>(string exportName, ExecutionEnvironment environment, IEnumerable<IPackageRepository> repositories) where T : IExport
        {
            var query = from repository in repositories
                        from packageName in repository.PackagesByName
                        let packageInfo = packageName.OrderByDescending(x => x.Version).FirstOrDefault()
                        select new { packageName, packageInfo };
            var latestPackages = from packageByName in query.ToLookup(x => x.packageName)
                                 select packageByName.FirstOrDefault().packageInfo.Load();

            return latestPackages.Select(x => x.GetExport(exportName, environment)).OfType<T>();
        }

        public void Initialize()
        {
        }
    }
}