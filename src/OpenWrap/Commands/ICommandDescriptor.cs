using System.Collections.Generic;

namespace OpenRasta.Wrap.Commands
{
    public interface ICommandDescriptor
    {
        string Namespace { get; }
        string Name { get; }
        string DisplayName { get; }
        string Description { get; }

        IDictionary<string, ICommandInputDescriptor> Inputs { get; }

        ICommand Create();
    }
}