using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using OpenWrap.Collections;
using OpenWrap.Commands;
using OpenWrap.PackageModel;
using OpenWrap.Reflection;
using OpenWrap.Repositories;
using OpenWrap.Runtime;
using OpenFileSystem.IO;
using ICustomAttributeProvider = Mono.Cecil.ICustomAttributeProvider;

namespace OpenWrap.PackageManagement.Exporters
{
    public class CommandManager
    {
        IPackageExporter _exporter;
        IPackageRepository _projectRepository;
        IPackageRepository _systemRepository;

        public CommandManager(IPackageExporter exporter, IEnvironment environment)
        {
            _exporter = exporter;
            _projectRepository = environment.ProjectRepository;
            _systemRepository = environment.SystemRepository;
        }
        public void Initialize()
        {

        }
    }

    public class CecilCommandInputDescriptor : ICommandInputDescriptor
    {
        string _propertyName;

        public CecilCommandInputDescriptor()
        {
        }

        public CecilCommandInputDescriptor(PropertyDefinition property, IDictionary<string, object> inputAttrib)
        {
            _propertyName = property.Name;
            inputAttrib.TryGet("Name", name => Name = (string)name);
            inputAttrib.TryGet("Description", description => Description = (string)description);
            inputAttrib.TryGet("IsValueRequired", _ => IsValueRequired = (bool)_);
            inputAttrib.TryGet("IsRequired", _ => IsRequired = (bool)_);
            inputAttrib.TryGet("Position", position => Position = (int)position);

            Name = Name ?? property.Name;
            Type = property.PropertyType.FullName;
        }

        public bool IsRequired { get; set; }
        public bool IsValueRequired { get; set; }

        public bool MultiValues
        {
            get { return false; }
        }

        public string Name { get; set; }

        public string Type { get; private set; }

        public string Description { get; set; }
        public int? Position { get; set; }


        public bool TrySetValue(ICommand target, IEnumerable<string> values)
        {
            var pi = target.GetType().GetProperty(_propertyName);
            var destinationType = pi.PropertyType;

            if (IsValueRequired == false && values.Count() == 0)
            {
                if (destinationType == typeof(bool))
                    pi.SetValue(target, true, null);
                else if (destinationType.IsValueType)
                    pi.SetValue(target, Activator.CreateInstance(destinationType), null);
                else
                    pi.SetValue(target, null, null);
                return true;
            }
            object value;

            if (!StringConversion.TryConvert(destinationType, values, out value))
                return false;
            try
            {
                pi.SetValue(target, value, null);
                return true;
            }
            catch
            {
            }
            return false;
        }
    }

    public static class StringConversion
    {
        delegate bool Converter(IEnumerable<string> values, out object result);

        static readonly Dictionary<Type, Converter> _converters = new Dictionary<Type, Converter>
        {
            {typeof(string), ConvertString},
            {typeof(IEnumerable<string>), ConvertListOfString}
        };

        static bool ConvertListOfString(IEnumerable<string> values, out object result)
        {
            result = values;
            return true;
        }

        static bool ConvertString(IEnumerable<string> values, out object result)
        {
            result = null;
            if (values.Count() != 1) return false;
            result = values.First();
            return true;
        }
        static Converter FallbackConverter(Type destinationType)
        {
            return (IEnumerable<string> values, out object result) => ConvertObject(destinationType, values, out result);
        }
        public static bool TryConvert(Type destinationType, IEnumerable<string> values, out object result)
        {
            return GetConverter(destinationType)(values, out result);
        }

        static Converter GetConverter(Type destinationType)
        {
            return _converters.ContainsKey(destinationType)
                           ? _converters[destinationType]
                           : FallbackConverter(destinationType);
        }

        static bool ConvertObject(Type destinationType, IEnumerable<string> values, out object result)
        {
            result = null;
            var enumerableTypes = (from type in destinationType.GetInterfaces()
                                   where type.IsGenericType
                                   let def = type.GetGenericTypeDefinition()
                                   where def == typeof(IEnumerable<>)
                                   select type.GetGenericArguments().Single()).ToList();
            var enumType = destinationType.IsInterface && destinationType.IsGenericType && destinationType.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                                   ? destinationType.GetGenericArguments().Single()
                                   : null;
            if (enumType != null)
            {
                var converter = GetConverter(enumType);
                var list = Activator.CreateInstance(typeof(List<>).MakeGenericType(enumType)) as IList;
                foreach (var value in values)
                {
                    object convertedValue;
                    if (converter(new[] { value }, out convertedValue))
                        list.Add(convertedValue);
                    else return false;
                }
                result = list;
                return true;
            }
            if (values.Count() != 1)
                return false;
            var valueToConvert = values.Single();
            try
            {
                result = Convert.ChangeType(valueToConvert, destinationType);
                return true;
            }
            catch { }
            var typeConverter = TypeDescriptor.GetConverter(destinationType);
            if (typeConverter == null || !typeConverter.CanConvertFrom(typeof(string))) return false;
            try
            {
                result = typeConverter.ConvertFromString(valueToConvert);
                return true;
            }
            catch { }
            return false;
        }
    }
    public class CommandExportItem : Exports.ICommand
    {
        public CommandExportItem(string path, IPackage package, ICommandDescriptor descriptor)
        {
            Path = path;
            Package = package;
            Descriptor = descriptor;
        }

        public string Path { get; private set; }
        public IPackage Package { get; private set; }
        public ICommandDescriptor Descriptor { get; private set; }
    }

    public class CommandDescriptor : ICommandDescriptor
    {
        readonly Func<ICommand> _factory;

        public CommandDescriptor(Func<ICommand> factory, string noun, string verb)
        {
            _factory = factory;
            Noun = noun;
            Verb = verb;
            Inputs = new Dictionary<string, ICommandInputDescriptor>();
        }
        protected CommandDescriptor(Func<ICommand> factory)
        {
            _factory = factory;
        }
        public virtual string Noun { get; protected set; }
        public virtual string Verb { get; protected set; }
        public virtual string Description { get; protected set; }
        public virtual IDictionary<string, ICommandInputDescriptor> Inputs { get; private set; }
        public virtual ICommand Create()
        {
            return _factory();
        }
    }
    public class CecilUICommandDescriptor : CommandDescriptor, Exports.ICommand
    {
        public CecilUICommandDescriptor(TypeDefinition typeDef, IDictionary<string, object> commandAttribute, IDictionary<string, object> uiAttribute, IEnumerable<CecilCommandInputDescriptor> inputs)
            : base(null)
        {
        }

        public string Path
        {
            get { throw new NotImplementedException(); }
        }

        public IPackage Package
        {
            get { throw new NotImplementedException(); }
        }

        public ICommandDescriptor Descriptor
        {
            get { throw new NotImplementedException(); }
        }
    }

    public class CecilCommandDescriptor : ICommandDescriptor
    {
        public CecilCommandDescriptor(TypeDefinition typeDef, IDictionary<string, object> commandAttribute, IEnumerable<CecilCommandInputDescriptor> inputs)
        {
            commandAttribute.TryGet("Noun", noun => Noun = (string)noun);
            commandAttribute.TryGet("Verb", verb => Verb = (string)verb);
            var tokenPrefix = Verb + "-" + Noun;
            commandAttribute.TryGet("Description", _ => Description = (string)_);
            Description = Description ?? CommandDocumentation.GetCommandDescription(tokenPrefix);
            Inputs = inputs.ToDictionary(x => x.Name,
                                         x =>
                                         {
                                             x.Description = x.Description ?? CommandDocumentation.GetCommandDescription(tokenPrefix + "-" + x.Name);
                                             return (ICommandInputDescriptor)x;
                                         }, StringComparer.OrdinalIgnoreCase);

            Factory = () => (ICommand)Activator.CreateInstance(Type.GetType(typeDef.FullName + "," + typeDef.Module.Assembly.FullName));
        }

        public Func<ICommand> Factory { get; set; }

        public string Noun { get; private set; }

        public string Verb { get; private set; }
        public string Description { get; private set; }

        public IDictionary<string, ICommandInputDescriptor> Inputs { get; set; }

        public ICommand Create()
        {
            return Factory();
        }
    }

    public static class CecilExtensions
    {
        public static Stream ToStream(this AssemblyDefinition def)
        {
            var ms = new MemoryStream();
            def.Write(ms);
            ms.Position = 0;
            return ms;
        }
        public static IDictionary<string, object> GetAttribute<T>(this ICustomAttributeProvider typeDef) where T : Attribute
        {
            var attribType = typeof(T);
            var attr = typeDef.CustomAttributes.Where(_ => _.AttributeType.Is<T>()).FirstOrDefault();
            return attr == null
                ? null
                : attr.Properties.ToDictionary(x => x.Name, x => x.Argument.Value);
        }
        public static IEnumerable<ExportedType> CommandTypes(this IEnumerable<ExportedType> types)
        {
            return from type in types
                   where type.IsAbstract == false
                   let typeDef = type.Resolve()
                   where typeDef.HasGenericParameters == false || typeDef.IsGenericInstance
                   select type;

        }
        public static bool Is<T>(this TypeReference reference)
        {
            var seekedType = typeof(T);
            AssemblyNameReference source;
            var assemblyMatches = (source = reference.Scope as AssemblyNameReference) != null ? seekedType.Assembly.FullName == source.FullName : false;
            return reference.FullName == seekedType.FullName && assemblyMatches;
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