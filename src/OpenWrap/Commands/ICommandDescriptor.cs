using System.Collections.Generic;
using OpenWrap.Commands;

namespace OpenWrap.Commands
{
    public interface ICommandDescriptor
    {
        string Namespace { get; }
        string Verb { get; }
        string DisplayName { get; }
        string Description { get; }

        IDictionary<string, ICommandInputDescriptor> Inputs { get; }

        ICommand Create();
    }
}