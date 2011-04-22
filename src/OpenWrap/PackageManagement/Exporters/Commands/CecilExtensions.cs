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
        public static bool Is<T>(this TypeReference reference)
        {
            var seekedType = typeof(T);
            AssemblyNameReference source;
            var assemblyMatches = (source = reference.Scope as AssemblyNameReference) != null ? seekedType.Assembly.FullName == source.FullName : false;
            return reference.FullName == seekedType.FullName && assemblyMatches;
        }
    }
}