using System;

namespace OpenWrap.Commands
{
    public interface ICommandInputDescriptor
    {
        bool IsRequired { get; }
        string Name { get; }
        Type Type { get; }
        string Description { get; }
        int? Position { get; }
        object ValidateValue<T>(T value);
        void SetValue(object target, object value);
    }
}