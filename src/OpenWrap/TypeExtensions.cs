using System;

namespace OpenWrap
{
    public static class TypeExtensions
    {
        public static T Attribute<T>(this Type type) where T:Attribute
        {
            return System.Attribute.GetCustomAttribute(type, typeof(T)) as T;
        }
    }
}