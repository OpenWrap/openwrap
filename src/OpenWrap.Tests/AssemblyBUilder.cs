using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using OpenFileSystem.IO;
using OpenWrap;
using OpenWrap.Collections;
using OpenWrap.PackageManagement.Exporters;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using OpCode = Mono.Cecil.Cil.OpCode;
using Path = System.IO.Path;
using TypeAttributes = Mono.Cecil.TypeAttributes;

namespace Tests
{
    public static class AssemblyBuilder
    {
        public static void CreateAssembly(Stream stream, string assemblyName, params Expression<Action<FluentTypeBuilder>>[] types)
        {
            AssemblyDefinition assembly = CreateAssembly(assemblyName, types);
            assembly.Write(stream);
        }

        static AssemblyDefinition CreateAssembly(string assemblyName, Expression<Action<FluentTypeBuilder>>[] types)
        {
            var assembly = AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition(assemblyName, "0.0.0.0".ToVersion()), assemblyName + ".dll", ModuleKind.Dll);
            foreach (var type in types)
            {
                var builder = new FluentTypeBuilder(assembly.MainModule, type.Parameters[0].Name);
                var config = type.Compile();
                config(builder);
                var typeDef = (TypeDefinition)builder;
                assembly.MainModule.Types.Add(typeDef);
                //assembly.MainModule.ExportedTypes.Add(new ExportedType(typeDef.Namespace, typeDef.Name, assembly.MainModule, assembly.Name));
            }
            return assembly;
        }

        public static Stream CreateAssemblyStream(string assemblyName, params Expression<Action<FluentTypeBuilder>>[] types)
        {
            var ms = new MemoryStream();
            CreateAssembly(ms, assemblyName, types);
            ms.Position = 0;
            return ms;
        }

        public static IFile CreateAssemblyStream(this IDirectory directory, string assemblyName)
        {
            var assemblyFile = directory.GetFile(assemblyName + ".dll");
            using (var assemblyStream = assemblyFile.OpenWrite())
                CreateAssembly(assemblyStream, assemblyName);
            return assemblyFile;
        }

        public static CustomAttribute CreateCustomAttribute<T>(ModuleDefinition module, Expression<Func<T>> attribute)
        {
            var init = attribute.Body as MemberInitExpression;
            if (init == null) throw new InvalidOperationException("Only constructing lambdas are supported");

            var ctor = init.NewExpression.Constructor;
            var ctorArgs = init.NewExpression.Arguments.Cast<ConstantExpression>().Select(x => new CustomAttributeArgument(module.Import(x.Type), x.Value));
            var ctorParams = init.Bindings.Cast<MemberAssignment>().Select(x =>
                                                                           new CustomAttributeNamedArgument(
                                                                                   x.Member.Name,
                                                                                   new CustomAttributeArgument(module.Import(x.Expression.Type), ((ConstantExpression)x.Expression).Value)));
            var attrib = new CustomAttribute(module.Import(ctor));
            attrib.ConstructorArguments.AddRange(ctorArgs);
            attrib.Properties.AddRange(ctorParams);
            return attrib;
        }

    }

    public delegate void FluentAssemblyBuilder(params Expression<Action<FluentTypeBuilder>>[] types);

    public class FluentTypeBuilder
    {
        readonly ModuleDefinition _module;
        readonly string _name;
        string _namespace;
        TypeAttributes _visibility;
        TypeReference _baseClass;
        List<FluentMethodBuilder> _methods = new List<FluentMethodBuilder>();
        List<CustomAttribute> _attributes = new List<CustomAttribute>();
        List<TypeReference> _interfaces = new List<TypeReference>();


        public FluentTypeBuilder(ModuleDefinition module, string name)
        {
            _module = module;
            _name = name;
            _visibility = TypeAttributes.Public;

        }

        public FluentTypeBuilder Internal
        {
            get
            {
                _visibility = TypeAttributes.NotPublic;
                return this;
            }
        }
        public FluentTypeBuilder Inherits<T>()
        {
            var targetType = typeof(T);

            _baseClass = _module.Import(typeof(T));
            return this;
        }
        public FluentTypeBuilder Methods(params Expression<Action<FluentMethodBuilder>>[] methods)
        {
            foreach (var method in methods)
            {
                var methodBuilder = new FluentMethodBuilder(_module, method.Parameters[0].Name);
                var config = method.Compile();
                config(methodBuilder);
                _methods.Add(methodBuilder);
            }
            return this;
        }
        public FluentTypeBuilder Properties(params Expression<Action<FluentPropertyBuilder>>[] properties)
        {
            return this;
        }
        public static explicit operator TypeDefinition(FluentTypeBuilder builder)
        {
            var td = builder._baseClass != null ? new TypeDefinition(builder._namespace, builder._name, GetTypeAttributes(builder), builder._baseClass) : new TypeDefinition(builder._namespace, builder._name, GetTypeAttributes(builder));
            td.Methods.AddRange(builder._methods.Select(_ => _.Build()));
            td.CustomAttributes.AddRange(builder._attributes);
            td.Interfaces.AddRange(builder._interfaces);
            return td;
        }

        public FluentTypeBuilder Namespace(string @namespace)
        {
            _namespace = @namespace;
            return this;
        }

        public FluentTypeBuilder Attribute<T>(Expression<Func<T>> attribute) where T : Attribute
        {
            CustomAttribute attrib = AssemblyBuilder.CreateCustomAttribute(_module, attribute);
            _attributes.Add(attrib);
            return this;
        }

        static TypeAttributes GetTypeAttributes(FluentTypeBuilder builder)
        {
            return builder._visibility | TypeAttributes.Class;
        }

        public FluentTypeBuilder Implements<T>()
        {
            var interfaceType = _module.Import(typeof(T));
            _interfaces.Add(interfaceType);
            return this;
        }
    }

    public class FluentPropertyBuilder
    {
        readonly ModuleDefinition _module;
        readonly string _name;
        TypeReference _returnType;
        bool _hasGetter;
        bool _hasSetter;
        List<CustomAttribute> _attributes = new List<CustomAttribute>();

        public FluentPropertyBuilder(ModuleDefinition module, string name)
        {
            _module = module;
            _name = name;
        }

        public FluentPropertyBuilder OfType<T>()
        {
            _returnType = _module.Import(typeof(T));
            return this;
        }
        public FluentPropertyBuilder Get()
        {
            _hasGetter = true;
            return this;
        }
        public FluentPropertyBuilder Set()
        {
            _hasSetter = true;
            return this;
        }

        public static explicit operator PropertyDefinition(FluentPropertyBuilder builder)
        {
            var pd = new PropertyDefinition(builder._name, PropertyAttributes.None, builder._returnType);
            if (builder._hasGetter)
                pd.GetMethod = new MethodDefinition("get_" + builder._name, MethodAttributes.Public, builder._returnType);
            if (builder._hasSetter)
                pd.SetMethod = new MethodDefinition("set_" + builder._name, MethodAttributes.Public, builder._returnType);

            return pd;
        }

        public FluentPropertyBuilder Attribute<T>(Expression<Func<T>> attribute) where T : Attribute
        {
            CustomAttribute attrib = AssemblyBuilder.CreateCustomAttribute(_module, attribute);
            _attributes.Add(attrib);
            return this;
        }
    }

    public class FluentMethodBuilder
    {
        readonly ModuleDefinition _module;
        readonly string _name;
        TypeReference _returnType;
        MethodAttributes _visibility;
        MethodAttributes _instance;
        object _returnValue;

        public FluentMethodBuilder(ModuleDefinition module, string name)
        {
            _module = module;
            _name = name;
            _visibility = MethodAttributes.Public;
            _returnType = _module.Import(typeof(void));
        }
        public FluentMethodBuilder Private
        {
            get
            {
                _visibility = MethodAttributes.Private;
                return this;
            }
        }
        public FluentMethodBuilder Static
        {
            get
            {
                _instance = MethodAttributes.Static;
                return this;
            }
        }
        public FluentMethodBuilder Return<T>()
        {
            _returnType = _module.Import(typeof(T));

            return this;
        }
        public MethodDefinition Build()
        {
            return new MethodDefinition(_name, _visibility | _instance, _returnType ?? _module.Import(typeof(void)));
        }
    }
}