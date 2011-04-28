using System.Collections.Generic;
using System.Linq;
using OpenWrap.PackageModel;
using OpenWrap.Runtime;

namespace OpenWrap.PackageManagement
{
    public static class PackageExporterExtensions
    {
        public static IEnumerable<Exports.IFile> Content(this IPackageExporter exporter, IPackage package)
        {
            return package.Content.SelectMany(_ => _);
        }
        public static IEnumerable<Exports.IAssembly> Assemblies(this IPackageExporter exporter, IPackage package, ExecutionEnvironment environment)
        {
            return exporter.Exports<Exports.IAssembly>(package, environment).SelectMany(x => x);
        }
    }
}