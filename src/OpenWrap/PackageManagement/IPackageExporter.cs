using System.Collections.Generic;
using OpenWrap.Repositories;
using OpenWrap.Runtime;
using OpenWrap.Services;

namespace OpenWrap.PackageManagement
{
    public interface IPackageExporter : IService
    {
        /// <summary>
        ///   Gets all the exports present in the provided repositories.
        /// </summary>
        /// <typeparam name = "T"></typeparam>
        /// <param name = "exportName"></param>
        /// <param name = "environment"></param>
        /// <param name = "repositories"></param>
        /// <returns></returns>
        IEnumerable<T> GetExports<T>(string exportName, ExecutionEnvironment environment, IEnumerable<IPackageRepository> repositories)
                where T : IExport;
    }
}