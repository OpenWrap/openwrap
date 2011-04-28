using System;
using System.Collections.Generic;
using Mono.Cecil;
using OpenWrap.Commands;

namespace OpenWrap.PackageManagement.Exporters
{
    public class CecilCommandDescriptor : ICommandDescriptor
    {
        public CecilCommandDescriptor(TypeDefinition typeDef, IDictionary<string, object> commandAttribute, IDictionary<string, ICommandInputDescriptor> inputs)
        {
            throw new NotImplementedException();
        }

        public string Noun
        {
            get { throw new NotImplementedException(); }
        }

        public string Verb
        {
            get { throw new NotImplementedException(); }
        }

        public string Description
        {
            get { throw new NotImplementedException(); }
        }

        public IDictionary<string, ICommandInputDescriptor> Inputs
        {
            get { throw new NotImplementedException(); }
        }

        public ICommand Create()
        {
            throw new NotImplementedException();
        }
    }
}