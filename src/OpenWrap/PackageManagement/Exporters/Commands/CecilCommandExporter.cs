using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using OpenWrap.Commands;
using OpenWrap.IO;
using OpenWrap.PackageManagement.Exporters.Assemblies;
using OpenWrap.Runtime;
using OpenWrap.Services;

namespace OpenWrap.PackageManagement.Exporters.Commands
{
    public class CecilCommandExporter : AbstractAssemblyExporter
    {
        public CecilCommandExporter(IEnvironment env)
            : base("commands", env.ExecutionEnvironment.Profile, env.ExecutionEnvironment.Platform)
        {
        }

        public override IEnumerable<IGrouping<string, TItem>> Items<TItem>(PackageModel.IPackage package)
        {
            if (typeof(TItem) != typeof(Exports.ICommand)) return Enumerable.Empty<IGrouping<string, TItem>>();

            var commandAssemblies = GetAssemblies<Exports.IAssembly>(package);

            return from source in commandAssemblies
                   from assembly in source
                   from type in ReadCommandTypes(assembly)
                           .Select(_ => new CommandExportItem(assembly.Path, package, _))
                           .Cast<TItem>()
                   group type by source.Key;
        }

        IEnumerable<ICommandDescriptor> ReadCommandTypes(Exports.IAssembly assembly)
        {
            return assembly.File.Read(GetCommandsFromAssembly);
        }

        public static IEnumerable<ICommandDescriptor> GetCommandsFromAssembly(Stream assemblyStream)
        {
            try
            {
                var module1 = AssemblyDefinition.ReadAssembly(assemblyStream, new ReaderParameters(ReadingMode.Deferred){AssemblyResolver = ServiceLocator.GetService<IAssemblyResolver>() ?? GlobalAssemblyResolver.Instance}).MainModule;
                return (from type in module1.Types
                        where type.IsPublic && type.IsAbstract == false && type.IsClass
                        let typeDef = type.Resolve()
                        where typeDef.HasGenericParameters == false || typeDef.IsGenericInstance
                        where typeDef.AllInterfaces().Any(_ => _.Is<ICommand>())
                        let cmd = GetCommandFromTypeDef(typeDef)
                        where cmd != null
                        select cmd
                       ).ToList();
            }
            catch
            {
                return Enumerable.Empty<ICommandDescriptor>();
            }
        }
        public static ICommandDescriptor GetCommandFrom<T>()
        {
            var typeDefDescriptor = GetCommandFromTypeDef(AssemblyDefinition.ReadAssembly(typeof(T).Assembly.Location).MainModule.Import(typeof(T)).Resolve());
            if (typeDefDescriptor == null) throw new NotSupportedException(string.Format("Type '{0}' doesn't appear to be a command.", typeof(T).FullName));
            return typeDefDescriptor;
        }

        public static ICommandDescriptor GetCommandFromTypeDef(TypeDefinition typeDef)
        {
            var commandAttribute = typeDef.GetAttribute<CommandAttribute>();
            var uiAttribute = typeDef.GetAttribute<UICommandAttribute>();
                        if( commandAttribute == null) return null;
            var inputs = ReadInputs(typeDef);
            return uiAttribute != null
                           ? (ICommandDescriptor)new CecilUICommandDescriptor(typeDef, commandAttribute, uiAttribute, inputs)
                           : (ICommandDescriptor)new CecilCommandDescriptor(typeDef, commandAttribute, inputs);
        }

        static IEnumerable<CecilCommandInputDescriptor> ReadInputs(TypeDefinition typeDef)
        {
            return (from property in typeDef.Properties
                    let inputAttrib = property.GetAttribute<CommandInputAttribute>()
                    where inputAttrib != null
                    select new CecilCommandInputDescriptor(property, inputAttrib)
                   );
        }
    }
}