using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using OpenWrap.Commands;
using OpenWrap.IO;
using OpenWrap.PackageManagement.Exporters.Assemblies;
using OpenWrap.PackageManagement.Exporters.Commands;
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
                   from solutionPlugin in assembly.File.Read(stream => Plugins(assembly.Path, stream)).Cast<TItem>()
                   group solutionPlugin by source.Key;

        }
        public IEnumerable<Exports.ISolutionPlugin> Plugins(string path, Stream assemblyStream)
        {
            try
            {
                var module1 = AssemblyDefinition.ReadAssembly(assemblyStream, new ReaderParameters(ReadingMode.Deferred)
                {
                    AssemblyResolver = ServiceLocator.GetService<IAssemblyResolver>() ?? GlobalAssemblyResolver.Instance
                }).MainModule;
                return (from type in module1.Types
                        where type.IsPublic && type.IsAbstract == false && type.IsClass && type.Name.EndsWith("Plugin")
                        select new SolutionPlugin(path, type)).Cast<Exports.ISolutionPlugin>().ToList();
            }
            catch
            {
                return Enumerable.Empty<Exports.ISolutionPlugin>();
            }
        }
    }

    public class SolutionPlugin : Exports.ISolutionPlugin
    {
        readonly TypeDefinition _type;

        public SolutionPlugin(string path,TypeDefinition type)
        {
            _type = type;
            Path = path;
        }

        public string Path { get; private set; }

        public IPackage Package { get; private set; }

        public string Name { get; private set; }

        public IDisposable Start()
        {
            var plugin = Activator.CreateInstanceFrom(_type.Module.FullyQualifiedName, _type.FullName).Unwrap();
            if (plugin is IDisposable)
                return (IDisposable)plugin;
            return new PluginWrapper(plugin);
        }

        class PluginWrapper : IDisposable
        {
            readonly object _plugin;

            public PluginWrapper(object plugin)
            {
                _plugin = plugin;
            }

            public void Dispose()
            {
            }
        }
    }
}