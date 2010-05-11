using System.Collections.Generic;
using System.Linq;
using OpenRasta.Wrap.Dependencies;
using OpenRasta.Wrap.Repositories;
using OpenRasta.Wrap.Sources;

namespace OpenRasta.Wrap.Build.Services
{
    public class WrapDependencyResolver
    {
        public IEnumerable<IAssemblyReferenceExportItem> GetAssemblyReferences(WrapDescriptor descriptor, IWrapRepository repository, IWrapAssemblyClient client)
        {
            return (from dependency in descriptor.Dependencies
                    let package = repository.Find(dependency)
                    where package != null
                    let items = package.GetExport("bin", client.Environment).Items.OfType<IAssemblyReferenceExportItem>()
                    select items).SelectMany(x => x);
        }
    }
}