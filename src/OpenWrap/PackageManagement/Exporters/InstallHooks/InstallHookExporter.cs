using OpenWrap.PackageManagement.Exporters.Assemblies;

namespace OpenWrap.PackageManagement.Exporters.InstallHooks
{
    public class InstallHookExporter : AbstractAssemblyExporter
    {
        public InstallHookExporter() : base("install")
        {
        }
        public override System.Collections.Generic.IEnumerable<System.Linq.IGrouping<string, TItem>> Items<TItem>(PackageModel.IPackage package, Runtime.ExecutionEnvironment environment)
        {
            return base.Items<TItem>(package, environment);
        }
    }
}