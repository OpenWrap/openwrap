using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OpenWrap.Commands
{
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
                      let inputAttrib = ReflectionExtensions.GetAttribute<CommandInputAttribute>(pi)
                      where inputAttrib != null
                      let values = inputAttrib ?? new CommandInputAttribute()
                      select (ICommandInputDescriptor)new CommandInputDescriptor
                      {
                          Name = values.Name ?? pi.Name,
                          IsRequired = values.IsRequired,
                          DisplayName = values.DisplayName ?? StringExtensions.CamelToSpacedName(pi.Name),
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
}