using System;
using System.Reflection;

namespace OpenWrap
{
    public static class TypeExtensions
    {
        public static T Attribute<T>(this MemberInfo type) where T:Attribute
        {
            return System.Attribute.GetCustomAttribute(type, typeof(T)) as T;
        }
    }
}