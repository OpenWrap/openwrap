using System;
using System.Collections.Generic;
using Mono.Cecil;
using OpenWrap.Commands;
using OpenWrap.PackageModel;

namespace OpenWrap.PackageManagement.Exporters.Commands
{
    public class CecilUICommandDescriptor : CecilCommandDescriptor, IUICommandDescriptor
    {
        public CecilUICommandDescriptor(TypeDefinition typeDef, IDictionary<string, object> commandAttribute, IDictionary<string, object> uiAttribute, IEnumerable<CecilCommandInputDescriptor> inputs)
                : base(typeDef, commandAttribute,inputs)
        {
            uiAttribute.TryGet("Label", label => Label = (string)label);
            uiAttribute.TryGet("Context", context => Context = (UICommandContext)context);

        }

        public string Label { get; private set; }

        public UICommandContext Context { get; private set; }
    }
}