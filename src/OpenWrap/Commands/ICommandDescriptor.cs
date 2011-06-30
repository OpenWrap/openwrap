using System.Collections.Generic;

namespace OpenWrap.Commands
{
    public interface ICommandDescriptor
    {
        string Noun { get; }
        string Verb { get; }
        string Description { get; }

        IDictionary<string, ICommandInputDescriptor> Inputs { get; }
        bool Visible { get; }
        bool IsDefault { get; set; }

        ICommand Create();
    }
}