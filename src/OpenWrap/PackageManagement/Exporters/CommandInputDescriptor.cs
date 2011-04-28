using System;
using OpenWrap.Commands;

namespace OpenWrap.PackageManagement.Exporters
{
    public class CommandInputDescriptor : ICommandInputDescriptor
    {
        Func<object, object> _validate;

        public CommandInputDescriptor(Func<object, object> validate)
        {
            _validate = validate;
        }
        public bool IsRequired { get; set; }
        public bool IsValueRequired { get; set; }
        public string Name { get; set; }
        public Type Type { get; set; }
        public string Description { get; set; }
        public int? Position { get; set; }
        public object ValidateValue<T>(T value)
        {
            return _validate(value);

        }

        public void SetValue(object target, object value)
        {
            target.GetType().GetProperty(Name).SetValue(target, value, null);
        }
    }
}