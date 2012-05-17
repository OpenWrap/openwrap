using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace OpenWrap.PackageManagement.Exporters
{
    public static class StringConversion
    {
        delegate bool Converter(IEnumerable<string> values, out object result);

        static readonly Dictionary<Type, Converter> _converters = new Dictionary<Type, Converter>
        {
                {typeof(string), ConvertString},
                {typeof(IEnumerable<string>), ConvertListOfString},
                {typeof(Version), ConvertVersion},
                {typeof(SemanticVersion), ConvertSemanticVersion},
        };

        static bool ConvertVersion(IEnumerable<string> values, out object result)
        {
            result = null;
            if (values.Count() != 1) return false;
           
            try
            {
                result = new Version(values.Single());
            }
            catch
            {
                return false;
            }
            return true;
        }

        static bool ConvertSemanticVersion(IEnumerable<string> values, out object result)
        {
            result = null;
            if (values.Count() != 1) return false;

            try
            {
                result = SemanticVersion.TryParseExact(values.Single());
            }
            catch
            {
                return false;
            }
            return result != null;
        }

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
}