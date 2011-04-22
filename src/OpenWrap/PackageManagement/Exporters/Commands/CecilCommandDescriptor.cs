using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using OpenWrap.Commands;

namespace OpenWrap.PackageManagement.Exporters.Commands
{
    public class CecilCommandDescriptor : ICommandDescriptor
    {
        public CecilCommandDescriptor(TypeDefinition typeDef, IDictionary<string, object> commandAttribute, IEnumerable<CecilCommandInputDescriptor> inputs)
        {
            commandAttribute.TryGet("Noun", noun => Noun = (string)noun);
            commandAttribute.TryGet("Verb", verb => Verb = (string)verb);
            var tokenPrefix = Verb + "-" + Noun;
            commandAttribute.TryGet("Description", _ => Description = (string)_);
            Description = Description ?? CommandDocumentation.GetCommandDescription(typeDef.Module.Assembly, tokenPrefix);
            Inputs = inputs.ToDictionary(x => x.Name,
                                         x =>
                                         {
                                             x.Description = x.Description ?? CommandDocumentation.GetCommandDescription(typeDef.Module.Assembly, tokenPrefix + "-" + x.Name);
                                             return (ICommandInputDescriptor)x;
                                         }, StringComparer.OrdinalIgnoreCase);

            Factory = () => (ICommand)Activator.CreateInstance(Type.GetType(typeDef.FullName + "," + typeDef.Module.Assembly.FullName));
        }

        public Func<ICommand> Factory { get; set; }

        public string Noun { get; private set; }

        public string Verb { get; private set; }
        public string Description { get; private set; }

        public IDictionary<string, ICommandInputDescriptor> Inputs { get; set; }

        public ICommand Create()
        {
            return Factory();
        }
    }
}