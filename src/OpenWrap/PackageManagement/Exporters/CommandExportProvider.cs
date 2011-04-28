using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using OpenWrap.Collections;
using OpenWrap.Commands;
using OpenWrap.IO;
using OpenWrap.Reflection;
using OpenWrap.Runtime;
using OpenFileSystem.IO;

namespace OpenWrap.PackageManagement.Exporters
{
    public class CommandExportProvider : AbstractAssemblyExporter
    {
        public CommandExportProvider(IEnvironment env)
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
                    var module1 = AssemblyDefinition.ReadAssembly((Stream)assemblyStream, new ReaderParameters(ReadingMode.Deferred)).MainModule;
                    return (from type in module1.ExportedTypes
                            where type.IsAbstract == false && type.IsClass
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
                    select (ICommandInputDescriptor)new CommandInputDescriptor(ValidateValue(property))
                    {
                            //Description = inputAttrib.Contains()
                    }
                   ).ToDictionary(x => x.Name);
        }
        Func<object, object> ValidateValue(PropertyDefinition property)
        {
            return input =>
            {
                throw new NotImplementedException();
                //try
                //{
                //    if (property.PropertyType.Is<bool>())
                //    {
                //        return input == null ? true : typeof(bool).CreateInstanceFrom(input as string);
                //    }

                //    if (input is string)
                //    {
                //        return Property.PropertyType.CreateInstanceFrom(value as string);
                //    }
                //    if (property.PropertyType.Resolve().IsAssignableFrom(typeof(T)))
                //    {
                //        return value;
                //    }
                //}
                //catch
                //{
                //}
                //return null;
            };
        }
    }

    //internal class CommandExportBuilder
    //        : IExportBuilder
    //{
    //    IEnumerable<ICommandDescriptorProvider> _descriptorProviders = new[]
    //    {
    //            new AttributeBasedCommandProvider()
    //    };
    //    public string ExportName
    //    {
    //        get { return "commands"; }
    //    }

    //    public bool CanProcessExport(string exportName)
    //    {
    //        return ExportName.EqualsNoCase(exportName);
    //    }

    //    // TODO: Make sure assemblies already loaded are loaded from normal reflection context
    //    public IExport ProcessExports(IEnumerable<IExport> exports, ExecutionEnvironment environment)
    //    {
    //        if (exports.Any() == false) return null;
    //        var loadedAssemblyPaths = AppDomain.CurrentDomain.GetAssemblies()
    //                .Select(x => new { loc = TryGetAssemblyLocation(x), asm = x })
    //                .Where(x => x.loc != null)
    //                .ToLookup(x => x.loc, x => x.asm, StringComparer.OrdinalIgnoreCase);

    //        var reflectionOnlyContext = (from folder in exports
    //                                     from file in folder.Items
    //                                     where file.FullPath.EndsWith(".dll")
    //                                     let assembly = loadedAssemblyPaths.Contains(file.FullPath)
    //                                                           ? loadedAssemblyPaths[file.FullPath].First()
    //                                                           : TryReflectionOnlyLoad(file)
    //                                     where assembly != null
    //                                     select new { file, assembly }).ToList();

    //        var reflectionOnlyAssembliesWithCommandTypes =
    //               (
    //                    from asmFile in reflectionOnlyContext
    //                    let types = from type in TryGetExportedTypes(asmFile.assembly)
    //                                where type.IsAbstract == false &&
    //                                      type.IsGenericTypeDefinition == false &&
    //                                      TryGet(() => type.GetInterface("ICommand")) != null
    //                                select type
    //                    where types.Any()
    //                    let assembly = TryGet(() => System.Reflection.Assembly.LoadFrom(asmFile.file.FullPath))
    //                    where assembly != null
    //                    select new { assembly, asmFile.file, types = types.NotNull().ToList() }
    //               ).ToList();
    //        var commandTypes = (from commands in reflectionOnlyAssembliesWithCommandTypes
    //                            from type in commands.types
    //                            let loadFromContextType = TryGet(() => commands.assembly.GetType(type.FullName))
    //                            where loadFromContextType != null
    //                            select loadFromContextType).ToList();
    //        return new CommandExport(_descriptorProviders, commandTypes);
    //    }

    //    T TryGet<T>(Func<T> func)
    //    {
    //        try
    //        {
    //            return func();
    //        }
    //        catch
    //        {
    //            return default(T);
    //        }
    //    }
    //    static string TryGetAssemblyLocation(System.Reflection.Assembly assembly)
    //    {
    //        {
    //            try
    //            {
    //                return Path.GetFullPath(assembly.Location);
    //            }
    //            catch
    //            {
    //                return null;
    //            }
    //        }
    //    }

    //    static IEnumerable<Type> TryGetExportedTypes(System.Reflection.Assembly assembly)
    //    {
    //        try
    //        {
    //            return assembly.GetExportedTypes();
    //        }
    //        catch (Exception e)
    //        {
    //            Debug.WriteLine(string.Format("Could not get types from '{0}' because of '{1}'.", assembly.FullName, e.Message));
    //            return Enumerable.Empty<Type>();
    //        }
    //    }

    //    System.Reflection.Assembly TryReflectionOnlyLoad(IExportItem file)
    //    {

    //        try
    //        {
    //            var loadedAsm = System.Reflection.Assembly.ReflectionOnlyLoadFrom(file.FullPath);
    //            return loadedAsm;
    //        }
    //        catch (Exception e)
    //        {
    //            Debug.WriteLine(string.Format("Could not load assembly '{0}' because of '{1}'.", file.FullPath, e.Message));
    //            return null;
    //        }
    //    }
    //}
}