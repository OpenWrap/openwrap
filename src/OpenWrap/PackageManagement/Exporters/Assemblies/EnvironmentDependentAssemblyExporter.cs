using OpenWrap.Runtime;

namespace OpenWrap.PackageManagement.Exporters.Assemblies
{
    public class EnvironmentDependentAssemblyExporter : AbstractAssemblyExporter
    {
        public EnvironmentDependentAssemblyExporter(ExecutionEnvironment env, string exportName = "bin")
                : base(exportName, env.Profile, env.Platform)
        {
        }
    }
}