using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;

namespace OpenWrap.PackageManagement.Exporters.Commands
{
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
        public static IEnumerable<TypeReference> AllInterfaces(this TypeDefinition typeDefinition)
        {
            var current = typeDefinition;
            do
            {
                foreach (var i in current.Interfaces) yield return i;
                try
                {
                    current = current.BaseType.FullName != typeof(object).FullName ? current.BaseType.Resolve() : null;
                }
                catch (AssemblyResolutionException)
                {
                    current = null;
                }
            } while (current != null);
        }
        public static bool Is<T>(this TypeReference reference)
        {
            var seekedType = typeof(T);
            string referenceAssembly = null;
            if (reference.Scope.MetadataScopeType == MetadataScopeType.AssemblyNameReference)
                referenceAssembly = ((AssemblyNameReference)reference.Scope).Name;
            else if (reference.Scope.MetadataScopeType == MetadataScopeType.ModuleDefinition)
                referenceAssembly = ((ModuleDefinition)reference.Scope).Assembly.Name.Name;
            if (referenceAssembly == null) return false;
            return reference.FullName == seekedType.FullName && referenceAssembly == seekedType.Assembly.GetName().Name;
        }
    }
}