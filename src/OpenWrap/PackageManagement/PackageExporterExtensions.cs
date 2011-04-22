using System.Collections.Generic;
using System.Linq;
using OpenWrap.PackageModel;

namespace OpenWrap.PackageManagement
{
    public static class PackageExporterExtensions
    {
        public static IEnumerable<Exports.IFile> Content(this IPackageExporter exporter, IPackage package)
        {
            return package.Content.SelectMany(_ => _);
        }
        public static IEnumerable<Exports.IAssembly> Assemblies(this IPackageExporter exporter, IPackage package)
        {
            return exporter.Exports<Exports.IAssembly>(package).SelectMany(x => x);
        }
    }
}