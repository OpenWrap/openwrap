using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using OpenWrap.Commands;
using OpenWrap.IO;
using OpenWrap.Runtime;

namespace OpenWrap.PackageManagement.Exporters
{
    public class CecilCommandExportProvider : AbstractAssemblyExporter
    {
        public CecilCommandExportProvider(IEnvironment env)
                : base("commands", env.ExecutionEnvironment.Profile, env.ExecutionEnvironment.Platform)
        {

        }
        public override IEnumerable<IGrouping<string, TItem>> Items<TItem>(PackageModel.IPackage package)
        {
            if (typeof(TItem) != typeof(Exports.ICommand)) return Enumerable.Empty<IGrouping<string, TItem>>();

            var commandAssemblies = base.GetAssemblies<Exports.IAssembly>(package);

            return from source in commandAssemblies
                   from assembly in source
                   from type in ReadCommandTypes(assembly)
                           .Select(_ => new CommandExportItem(assembly.Path, package, _))
                           .Cast<TItem>()
                   group type by source.Key;
        }

        IEnumerable<ICommandDescriptor> ReadCommandTypes(Exports.IAssembly assembly)
        {
            return assembly.File.Read(assemblyStream=>
            {
                try
                {
                    var module1 = AssemblyDefinition.ReadAssembly(assemblyStream, new ReaderParameters(ReadingMode.Deferred)).MainModule;
                    return (from type in module1.Types
                            where type.IsPublic && type.IsAbstract == false && type.IsClass
                            let typeDef = type.Resolve()
                            where typeDef.HasGenericParameters == false || typeDef.IsGenericInstance
                            where typeDef.HasInterfaces && typeDef.Interfaces.Any(_ => _.Is<ICommand>())
                            let commandAttribute = typeDef.GetAttribute<CommandAttribute>()
                            let uiAttribute = typeDef.GetAttribute<UICommandAttribute>()
                            where commandAttribute != null
                            let inputs = ReadInputs(typeDef)
                            select uiAttribute != null
                                           ? (ICommandDescriptor)new CecilUICommandDescriptor(typeDef, commandAttribute, uiAttribute, inputs)
                                           : (ICommandDescriptor)new CecilCommandDescriptor(typeDef, commandAttribute, inputs)
                           ).ToList();
                }
                catch
                {
                    return Enumerable.Empty<ICommandDescriptor>();
                }
            });
        }

        IDictionary<string, ICommandInputDescriptor> ReadInputs(TypeDefinition typeDef)
        {
            return (from property in typeDef.Properties
                    let inputAttrib = property.GetAttribute<CommandInputAttribute>()
                    select (ICommandInputDescriptor)null//new CommandInputDescriptor()
                    //{
                    //        //Description = inputAttrib.Contains()
                    //}
                   ).ToDictionary(x => x.Name);
        }
    }
}