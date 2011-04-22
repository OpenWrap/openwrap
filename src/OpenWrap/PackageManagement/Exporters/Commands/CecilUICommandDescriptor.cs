using System;
using System.Collections.Generic;
using Mono.Cecil;
using OpenWrap.Commands;
using OpenWrap.PackageModel;

namespace OpenWrap.PackageManagement.Exporters.Commands
{
    public class CecilUICommandDescriptor : CommandDescriptor, Exports.ICommand
    {
        public CecilUICommandDescriptor(TypeDefinition typeDef, IDictionary<string, object> commandAttribute, IDictionary<string, object> uiAttribute, IEnumerable<CecilCommandInputDescriptor> inputs)
                : base(null)
        {
        }

        public string Path
        {
            get { throw new NotImplementedException(); }
        }

        public IPackage Package
        {
            get { throw new NotImplementedException(); }
        }

        public ICommandDescriptor Descriptor
        {
            get { throw new NotImplementedException(); }
        }
    }
}