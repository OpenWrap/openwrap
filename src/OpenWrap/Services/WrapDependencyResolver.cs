using System.Collections.Generic;
using System.Linq;
using OpenWrap.Exports;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;

namespace OpenWrap.Build.Services
{
    public class WrapDependencyResolver
    {
        public IEnumerable<IAssemblyReferenceExportItem> GetAssemblyReferences(WrapDescriptor descriptor, IPackageRepository repository, IWrapAssemblyClient client)
        {
            return (from dependency in descriptor.Dependencies
                    let package = repository.Find(dependency)
                    where package != null
                    let items = package.Load().GetExport("bin", client.Environment).Items.OfType<IAssemblyReferenceExportItem>()
                    select items).SelectMany(x => x);
        }
    }
}