using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OpenWrap.Reflection;

namespace OpenWrap.Commands
{
    public class ReflectionCommandInputDescriptor : ICommandInputDescriptor
    {
        public ReflectionCommandInputDescriptor(PropertyInfo property)
        {
            Property = property;
            IsValueRequired = true;
        }

        public string Description { get; set; }

        public bool IsRequired { get; set; }

        public bool IsValueRequired { get; set; }

        public bool MultiValues { get; set; }

        public string Name { get; set; }
        public int? Position { get; set; }
        

        public PropertyInfo Property { get; private set; }

        public string Type
        {
            get { return Property.PropertyType.FullName; }
        }

        public bool TrySetValue(ICommand target, IEnumerable<string> value)
        {
            Property.SetValue(target,
                              value.FirstOrDefault() ??
                              (Property.PropertyType == typeof(bool)
                                       ? "true"
                                       : null),
                              null);
            return true;
        }

        public object ValidateValue<T>(T value)
        {
            try
            {
                if (Property.PropertyType == typeof(bool))
                {
                    return value == null ? true : Property.PropertyType.CreateInstanceFrom(value as string);
                }

                if (value is string)
                {
                    return Property.PropertyType.CreateInstanceFrom(value as string);
                }
                if (Property.PropertyType.IsAssignableFrom(typeof(T)))
                {
                    return value;
                }
            }
            catch
            {
            }
            return null;
        }
    }
}