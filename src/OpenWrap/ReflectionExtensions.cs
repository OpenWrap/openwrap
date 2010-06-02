using System;
using System.Reflection;
using OpenWrap.Reflection;

namespace OpenWrap
{
    public static class ReflectionExtensions
    {
        public static T GetAttribute<T>(this MemberInfo member) where T:Attribute
        {
            return Attribute.GetCustomAttribute(member, typeof(T)) as T;
        }
        public static bool TrySetValue<T>(this PropertyInfo property, object target, T value)
        {
            try
            {
                if (value is string)
                {
                    var valueToAssign = property.PropertyType.CreateInstanceFrom(value as string);
                    if (valueToAssign == null) return false;
                    property.SetValue(target,valueToAssign,null);
                    return true;
                }
                if (property.PropertyType.IsAssignableFrom(value.GetType()))
                {
                    property.SetValue(target, value,null);
                    return true;
                }
            }
            catch
            {
            }
            return false;
        }
    }
}