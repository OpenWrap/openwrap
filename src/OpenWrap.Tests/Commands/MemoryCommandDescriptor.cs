using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.Commands;

namespace Tests.Commands
{
    public class MemoryCommandDescriptor : ICommandDescriptor
    {
        public string Noun { get; set; }
        public string Verb { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public IDictionary<string, ICommandInputDescriptor> Inputs { get { return AllInputs.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase); } }

        public bool Visible { get; set; }

        public IList<ICommandInputDescriptor> AllInputs { get; private set; }

        public MemoryCommandDescriptor()
        {
            AllInputs = new List<ICommandInputDescriptor>();
            Create = () => null;
        }

        public MemoryCommandDescriptor(IEnumerable<ICommandInputDescriptor> inputs)
        {
            AllInputs = new List<ICommandInputDescriptor>(inputs);

        }

        public Func<ICommand> Create { get; set; }

        public bool IsDefault { get; set; }

        ICommand ICommandDescriptor.Create()
        {
            return Create();
        }
    }
}