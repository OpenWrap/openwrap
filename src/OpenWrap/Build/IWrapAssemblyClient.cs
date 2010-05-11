using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenRasta.Wrap.Dependencies;
using OpenRasta.Wrap.Repositories;

namespace OpenRasta.Wrap.Build
{
    public interface IWrapAssemblyClient
    {
        void WrapAssembliesUpdated(IEnumerable<IAssemblyReferenceExportItem> assemblyPaths);
        WrapRuntimeEnvironment Environment { get; }
        bool IsLongRunning { get; }
    }
}
