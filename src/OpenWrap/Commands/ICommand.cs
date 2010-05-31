using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OpenWrap.Reflection;
using OpenWrap.Commands;

namespace OpenWrap.Commands
{
    public interface ICommand
    {
        IEnumerable<ICommandResult> Execute();
    }

    public class AttributeBasedCommandDescriptor : ICommandDescriptor
    {
        readonly Type _commandType;

        public AttributeBasedCommandDescriptor(Type commandType)
        {
            _commandType = commandType;
            var attribute = commandType.GetAttribute<CommandAttribute>() ?? new CommandAttribute();
            Noun = attribute.Namespace ?? commandType.Namespace;
            Verb = attribute.Verb ?? commandType.Name;
            DisplayName = attribute.DisplayName ?? commandType.Name.CamelToSpacedName();
            Description = attribute.Description ?? string.Empty;

            Inputs = (from pi in commandType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                      let inputAttrib = pi.GetAttribute<CommandInputAttribute>()
                      where inputAttrib != null
                      let values = inputAttrib ?? new CommandInputAttribute()
                      select (ICommandInputDescriptor)new CommandInputDescriptor
                      {
                          Name = values.Name ?? pi.Name,
                          IsRequired = values.IsRequired,
                          DisplayName = values.DisplayName ?? pi.Name.CamelToSpacedName(),
                          Position = values.Position,
                          Property = pi
                      }).ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        }

        public string Noun { get; set; }
        public string Verb { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public IDictionary<string, ICommandInputDescriptor> Inputs { get; set; }
        public ICommand Create()
        {
            return (ICommand)Activator.CreateInstance(_commandType);
            
        }
    }
    public class AttributeBasedCommandDescriptor<T> : AttributeBasedCommandDescriptor
        where T:ICommand
    {
        public AttributeBasedCommandDescriptor() : base(typeof(T))
        {
        }
    }
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandAttribute : Attribute
    {
        public string Namespace { get; set; }
        public string Verb { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
    }
    [AttributeUsage(AttributeTargets.Property)]
    public class CommandInputAttribute : Attribute
    {
        public CommandInputAttribute()
        {
            Position = -1;
        }
        public bool IsRequired
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }
        public string DisplayName
        {
            get;
            set;
        }
        public int Position
        {
            get; set;
        }
    }
    public interface ICommandInputDescriptor
    {
        bool IsRequired { get; }
        string Name { get; }
        string DisplayName { get; }
        int Position { get; }
        object ValidateValue<T>(T value);
        void SetValue(object target, object value);
    }
    public class CommandInputDescriptor : ICommandInputDescriptor
    {
        public bool IsRequired { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }


        public object ValidateValue<T>(T value)
        {
            try
            {
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
            Property.SetValue(target, value, null);
        }

        public PropertyInfo Property { get; set; }

        public int Position { get; set; }
    }

    public enum CommandContext
    {
        Always,
        Project,
        Module
    }
    public static class StringExtensions
    {
        public static string CamelToSpacedName(this string str)
        {
            //TODO: Evil not implemented yet.
            return str;
        }
    }
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
