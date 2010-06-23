using System.Reflection;
using OpenWrap.Reflection;

namespace OpenWrap.Commands
{
    public class CommandInputDescriptor : ICommandInputDescriptor
    {
        public bool IsRequired { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }


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
            Property.SetValue(target, value ?? true, null);
        }

        public PropertyInfo Property { get; set; }

        public int Position { get; set; }
    }
}