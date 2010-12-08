using System;
using System.Reflection;

namespace OpenWrap.Commands
{
    public class ReflectionCommandInputDescriptor : ICommandInputDescriptor
    {
        public ReflectionCommandInputDescriptor(PropertyInfo property)
        {
            Property = property;
        }

        public bool IsRequired { get; set; }

        public bool IsValueRequired { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public Type Type { get { return Property.PropertyType; } }
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
                if (Property.PropertyType.IsAssignableFrom(value.GetType()))
                {
                    return value;
                }
            }
            catch
            {
            }
            return null;
        }

        public void SetValue(object target, object value)
        {
            Property.SetValue(target, value ?? "true", null);
        }

        public PropertyInfo Property { get; private set; }

        public int? Position { get; set; }
    }
}