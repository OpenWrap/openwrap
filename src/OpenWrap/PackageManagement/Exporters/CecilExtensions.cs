using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace OpenWrap.PackageManagement.Exporters
{
    public static class CecilExtensions
    {

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
            return reference.Name == seekedType.Name && reference.Namespace == seekedType.Namespace && reference.Module.Assembly.Name.Name == seekedType.Assembly.GetName().Name;
        }
    }
}