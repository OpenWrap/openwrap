using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace OpenWrap.Reflection
{
    public static class SharedReflectionExtensions
    {
        /// <summary>
        ///   Creates a type using the provided string to initialize its values.
        /// </summary>
        /// <param name = "type">The type of object to create.</param>
        /// <param name = "propertyValue">The value to assign to the created object.</param>
        /// <returns>The created object.</returns>
        public static object CreateInstanceFrom(this Type type, string propertyValue)
        {
            return CreateInstanceFromString(type, propertyValue, null);
        }

        static object CreateInstanceFromString(this Type type, string propertyValue, Stack<Type> recursionDefender)
        {
            if (type == null || propertyValue == null) return null;
            if (type == typeof(string))
                return propertyValue;

            if (type == typeof(bool))
            {
                switch (propertyValue)
                {
                    case "0":
                    case "-0":
                    case "false":
                    case "off":
                    case "null":
                    case "NaN":
                    case "undefined":
                    case "":
                        return false;
                    default:
                        return true;
                }
            }

            if (type.IsPrimitive)
            {
                try
                {
                    return Convert.ChangeType(propertyValue, type);
                }
                catch
                {
                }
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICollection<>))
            {
                var collectionArg = type.GetGenericArguments()[0];
                var list = Activator.CreateInstance(typeof(List<>).MakeGenericType(collectionArg));
                var add = list.GetType().GetMethod("Add");
                foreach(var value in propertyValue.Split(new[]{","}, StringSplitOptions.RemoveEmptyEntries))
                {
                    var convertedValue = collectionArg.CreateInstanceFrom(value);
                    add.Invoke(list, new object[]{convertedValue});
                }
                return list;
            }
            recursionDefender = recursionDefender ?? new Stack<Type>();
            foreach (var constructor in type.GetConstructors())
            {
                var parameters = constructor.GetParameters();
                if (parameters.Length != 1) continue;
                object value;
                if (recursionDefender.Contains(parameters[0].ParameterType))
                    continue;
                recursionDefender.Push(type);
                try
                {
                    value = CreateInstanceFromString(parameters[0].ParameterType, propertyValue, recursionDefender);
                }
                catch
                {
                    continue;
                }
                finally
                {
                    recursionDefender.Pop();
                }

                if (value != null)
                    return constructor.Invoke(new[] { value });
            }

#if !SILVERLIGHT
            var converter = TypeDescriptor.GetConverter(type);
            if (converter == null || !converter.CanConvertFrom(typeof(string)))
                throw new InvalidCastException("Cannot convert the string \"" + propertyValue + "\" to type "
                                               + type.Name);
            return TypeDescriptor.GetConverter(type).ConvertFromInvariantString(propertyValue);
#else
                return null;
#endif
        }
    }
}