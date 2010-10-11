using System.Collections.Generic;
using System.Linq;
using OpenWrap.Build;
using OpenWrap.Exports;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;

namespace OpenWrap.Services
{
    public class PackageAssemblyResolver
    {
        public IEnumerable<IAssemblyReferenceExportItem> GetAssemblyReferences(PackageDescriptor descriptor, IPackageRepository repository, IWrapAssemblyClient client)
        {
            return (from dependency in descriptor.Dependencies
                    where dependency.ContentOnly == false
                    let package = repository.Find(dependency)
                    where package != null
                    let items = package.Load().GetExport("bin", client.Environment).Items.OfType<IAssemblyReferenceExportItem>()
                    select items).SelectMany(x => x);
        }
    }
}