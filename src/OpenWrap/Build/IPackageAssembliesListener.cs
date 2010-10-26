using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Exports;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;

namespace OpenWrap.Build
{
    public interface IPackageAssembliesListener
    {
        void AssembliesUpdated(IEnumerable<IAssemblyReferenceExportItem> assemblyPaths);
        ExecutionEnvironment Environment { get; }
        bool IsLongRunning { get; }
    }
}
