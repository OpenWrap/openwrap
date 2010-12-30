using System.Collections.Generic;
using OpenWrap.Commands;

namespace OpenWrap.Windows.NounVerb
{
    internal class NullCommandDescriptor : ICommandDescriptor
    {
        public string Noun { get; private set; }
        public string Verb { get; private set; }
        public string DisplayName { get; private set; }
        public string Description { get; private set; }
        public IDictionary<string, ICommandInputDescriptor> Inputs { get; private set; }

        public ICommand Create()
        {
            return null;
        }
    }
}