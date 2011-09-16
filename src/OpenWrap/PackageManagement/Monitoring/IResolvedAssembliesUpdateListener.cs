using System.Collections.Generic;
using OpenWrap.PackageManagement.Exporters;
using OpenWrap.Runtime;

namespace OpenWrap.PackageManagement.Monitoring
{
    public interface IResolvedAssembliesUpdateListener
    {
        void AssembliesUpdated(IEnumerable<Exports.IAssembly> assemblyPaths);
        ExecutionEnvironment Environment { get; }
        bool IsLongRunning { get; }
        void AssembliesError(string errorMessage);
    }
}