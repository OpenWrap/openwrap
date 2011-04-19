using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OpenWrap.Reflection;

namespace OpenWrap.Commands
{
    public class AttributeBasedCommandProvider : ICommandDescriptorProvider
   
    {
        public ICommandDescriptor TryGet(Type type)
        {
            var cmdAttribute = type.GetAttribute<CommandAttribute>();
            if (cmdAttribute == null) return null;
            var uiAttribute = type.GetAttribute<UICommandAttribute>();
            //if (uiAttribute != null)
            //    return new UIAttributeBasedCommandDescriptor(type, cmdAttribute, uiAttribute);
            //return new AttributeBasedCommandDescriptor(type, cmdAttribute);
            return null;


        }
    }

    //public class UIAttributeBasedCommandDescriptor : AttributeBasedCommandDescriptor, IUICommandDescriptor
    //{
    //    public UIAttributeBasedCommandDescriptor(Func<ICommand> factory, CommandAttribute cmdAttribute, UICommandAttribute uiAttribute)
    //        : base(factory, cmdAttribute)
    //    {
    //        Label = uiAttribute.Label;
    //        Context = uiAttribute.Context;
    //    }

    //    public string Label { get; set; }

    //    public UICommandContext Context { get; set; }
    //}

    //public class AttributeBasedCommandDescriptor : ICommandDescriptor
    //{
    //    readonly Type _commandType;

    //    public AttributeBasedCommandDescriptor(Func<ICommand> factory, CommandAttribute commandAttribute)
    //    {
    //        throw new NotImplementedException();
    //        //_commandType = commandType;

    //        //Noun = commandAttribute.Noun ?? DeductNounFromNamespace(commandType);
    //        //Verb = commandAttribute.Verb ?? commandType.Name;

    //        //var commandResourceKey = commandAttribute.Verb + "-" + commandAttribute.Noun;
    //        //Description = commandAttribute.Description ?? CommandDocumentation.GetCommandDescription(commandType, commandResourceKey);

    //        //Inputs = (from pi in commandType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
    //        //          let inputAttrib = pi.GetAttribute<CommandInputAttribute>()
    //        //          where inputAttrib != null
    //        //          let values = inputAttrib ?? new CommandInputAttribute()
    //        //          let inputName = values.Name ?? pi.Name
    //        //          select (ICommandInputDescriptor)new ReflectionCommandInputDescriptor(pi)
    //        //          {
    //        //              Name = inputName,
    //        //              IsRequired = values.IsRequired,
    //        //              Description = values.DisplayName ?? CommandDocumentation.GetCommandDescription(commandType, commandResourceKey + "-" + inputName),
    //        //              Position = values.Position == -1 ? (int?)null : values.Position,
    //        //              IsValueRequired = values.IsValueRequired
    //        //          }).ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
    //   }
    //}

    //public class UIAttributeBasedCommandDescriptor : AttributeBasedCommandDescriptor, IUICommandDescriptor
    //{
    //    public UIAttributeBasedCommandDescriptor(Func<ICommand> factory, CommandAttribute cmdAttribute, UICommandAttribute uiAttribute)
    //        : base(factory, cmdAttribute)
    //    {
    //        Label = uiAttribute.Label;
    //        Context = uiAttribute.Context;
    //    }

    //    public string Label { get; set; }

    //    public UICommandContext Context { get; set; }
    //}

    //public class AttributeBasedCommandDescriptor : ICommandDescriptor
    //{
    //    readonly Type _commandType;

    //    public AttributeBasedCommandDescriptor(Func<ICommand> factory, CommandAttribute commandAttribute)
    //    {
    //        throw new NotImplementedException();
    //        //_commandType = commandType;

    //        //Noun = commandAttribute.Noun ?? DeductNounFromNamespace(commandType);
    //        //Verb = commandAttribute.Verb ?? commandType.Name;

    //        //var commandResourceKey = commandAttribute.Verb + "-" + commandAttribute.Noun;
    //        //Description = commandAttribute.Description ?? CommandDocumentation.GetCommandDescription(commandType, commandResourceKey);

    //        //Inputs = (from pi in commandType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
    //        //          let inputAttrib = pi.GetAttribute<CommandInputAttribute>()
    //        //          where inputAttrib != null
    //        //          let values = inputAttrib ?? new CommandInputAttribute()
    //        //          let inputName = values.Name ?? pi.Name
    //        //          select (ICommandInputDescriptor)new ReflectionCommandInputDescriptor(pi)
    //        //          {
    //        //              Name = inputName,
    //        //              IsRequired = values.IsRequired,
    //        //              Description = values.DisplayName ?? CommandDocumentation.GetCommandDescription(commandType, commandResourceKey + "-" + inputName),
    //        //              Position = values.Position == -1 ? (int?)null : values.Position,
    //        //              IsValueRequired = values.IsValueRequired
    //        //          }).ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

    //    }
    //}

    //public class UIAttributeBasedCommandDescriptor : AttributeBasedCommandDescriptor, IUICommandDescriptor
    //{
    //    public UIAttributeBasedCommandDescriptor(Func<ICommand> factory, CommandAttribute cmdAttribute, UICommandAttribute uiAttribute)
    //        : base(factory, cmdAttribute)
    //    {
    //        Label = uiAttribute.Label;
    //        Context = uiAttribute.Context;
    //    }

    //    public string Label { get; set; }

    //    public UICommandContext Context { get; set; }
    //}

    //public class AttributeBasedCommandDescriptor : ICommandDescriptor
    //{
    //    readonly Type _commandType;

    //    public AttributeBasedCommandDescriptor(Func<ICommand> factory, CommandAttribute commandAttribute)
    //    {
    //        _commandType = commandType;
            
    //        Noun = commandAttribute.Noun ?? DeductNounFromNamespace(commandType);
    //        Verb = commandAttribute.Verb ?? commandType.Name;

    //        var commandResourceKey = commandAttribute.Verb + "-" + commandAttribute.Noun;
    //        Description = commandAttribute.Description ?? CommandDocumentation.GetCommandDescription(commandType, commandResourceKey);

    //        Inputs = (from pi in commandType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
    //                  let inputAttrib = pi.GetAttribute<CommandInputAttribute>()
    //                  where inputAttrib != null
    //                  let values = inputAttrib ?? new CommandInputAttribute()
    //                  let inputName = values.Name ?? pi.Name
    //                  select (ICommandInputDescriptor)new ReflectionCommandInputDescriptor(pi)
    //                  {
    //                          Name = inputName,
    //                          IsRequired = values.IsRequired,
    //                          Description = values.DisplayName ?? CommandDocumentation.GetCommandDescription(commandType, commandResourceKey + "-" + inputName),
    //                          Position = values.Position == -1 ? (int?)null : values.Position,
    //                          IsValueRequired = values.IsValueRequired
    //                  }).ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
    //    }

    //    public string Description { get; set; }
    //    public IDictionary<string, ICommandInputDescriptor> Inputs { get; set; }
    //    public string Noun { get; set; }
    //    public string Verb { get; set; }

    //    public ICommand Create()
    //    {
    //        return (ICommand)Activator.CreateInstance(_commandType);
    //    }

    //    string DeductNounFromNamespace(Type commandType)
    //    {
    //        int dotIndex = commandType.Namespace.LastIndexOf('.');
    //        return dotIndex != -1 ? commandType.Namespace.Substring(dotIndex = 1) : commandType.Namespace;
    //    }
    //}

    //public class AttributeBasedCommandDescriptor<T> : AttributeBasedCommandDescriptor
    //        where T : ICommand
    //{
    //    public AttributeBasedCommandDescriptor(CommandAttribute attribute) : base(typeof(T), attribute)
    //    {
    //    }
    //}
    //public class AttributeBasedCommandDescriptor<T> : AttributeBasedCommandDescriptor
    //        where T : ICommand
    //{
    //    public AttributeBasedCommandDescriptor(CommandAttribute attribute)
    //        : base(null, attribute)
    //    {
    //    }
    //}
}