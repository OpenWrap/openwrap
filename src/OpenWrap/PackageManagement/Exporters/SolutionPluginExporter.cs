using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using OpenWrap.IO;
using OpenWrap.PackageManagement.Exporters.Assemblies;
using OpenWrap.PackageModel;
using OpenWrap.Runtime;
using OpenWrap.Services;

namespace OpenWrap.PackageManagement.Exporters
{
    public class SolutionPluginExporter : AbstractAssemblyExporter
    {
        public SolutionPluginExporter() : base("solution")
        {
        }

        public override IEnumerable<IGrouping<string, TItem>> Items<TItem>(IPackage package, ExecutionEnvironment environment)
        {
            if (typeof(TItem) != typeof(Exports.ISolutionPlugin)) return Enumerable.Empty<IGrouping<string, TItem>>();

            return from source in GetAssemblies<Exports.IAssembly>(package, environment)
                   from assembly in source
                   from solutionPlugin in assembly.File.Read(stream => Plugins(package, assembly.Path, stream)).Cast<TItem>()
                   group solutionPlugin by source.Key;
        }

        static IEnumerable<Exports.ISolutionPlugin> Plugins(IPackage package, string path, Stream assemblyStream)
        {
            try
            {
                var module1 = AssemblyDefinition.ReadAssembly(assemblyStream,
                                                              new ReaderParameters(ReadingMode.Deferred)
                                                              {
                                                                  AssemblyResolver = ServiceLocator.GetService<IAssemblyResolver>() ?? GlobalAssemblyResolver.Instance
                                                              }).MainModule;
                return (from type in module1.Types
                        where type.IsPublic && type.IsAbstract == false && type.IsClass && type.Name.EndsWith("Plugin")
                        select new SolutionPlugin(package, path, type)).Cast<Exports.ISolutionPlugin>().ToList();
            }
            catch
            {
                return Enumerable.Empty<Exports.ISolutionPlugin>();
            }
        }
    }
}