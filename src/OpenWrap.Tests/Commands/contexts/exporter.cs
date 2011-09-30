using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Mono.Cecil;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenWrap;
using OpenWrap.IO.Packaging;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.Exporters;
using OpenWrap.PackageManagement.Packages;
using OpenWrap.Runtime;
using OpenWrap.Testing;
using OpenWrap.Tests.Commands;

namespace Tests.Commands.contexts
{
    public class exporter<TExporter, T> : context where T:IExportItem where TExporter : IExportProvider
    {
        InMemoryFileSystem FileSystem;
        protected IDirectory TempDirectory;
        //Dictionary<string,AssemblyDefinition[]> _assemblies = new Dictionary<string, AssemblyDefinition[]>();
        protected TExporter Exporter;
        protected IEnumerable<IGrouping<string, T>> Items;
        Dictionary<string, Dictionary<string, Expression<Action<FluentTypeBuilder>>[]>> _exports = new Dictionary<string, Dictionary<string, Expression<Action<FluentTypeBuilder>>[]>>((StringComparer.OrdinalIgnoreCase));

        public exporter()
        {
            FileSystem = new InMemoryFileSystem();
            TempDirectory = FileSystem.CreateTempDirectory();

        }
        protected void given_package_assembly(string exportName, Expression<Action<FluentAssemblyBuilder>> assembly)
        {
            var assemblyName = assembly.Parameters[0].Name;
            var builtTypes = new Expression<Action<FluentTypeBuilder>>[0];
            FluentAssemblyBuilder builder = types => builtTypes = types;

            assembly.Compile()(builder);
            _exports
                .GetOrCreate(exportName)
                [assemblyName] = builtTypes;
            
        }
        protected void when_exporting()
        {
            var package = Packager.NewWithDescriptor(TempDirectory.GetFile("tempName.wrap"), "tempName", "1.0", 
                from kv in _exports
                
                let export = kv.Key
                from assembly in kv.Value
                let fileName = assembly.Key + ".dll"
                select new PackageContent
                {
                        FileName = fileName,
                        RelativePath = export,
                        Stream = ()=> AssemblyBuilder.CreateAssemblyStream(assembly.Key, assembly.Value)
                });

            Items = Exporter.Items<T>(new ZipPackage(package).Load(), new ExecutionEnvironment("AnyCPU", "net35"));
        }
    }
}