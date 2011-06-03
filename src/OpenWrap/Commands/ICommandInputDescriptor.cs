using System;
using System.Collections.Generic;

namespace OpenWrap.Commands
{
    public interface ICommandInputDescriptor
    {
        bool IsRequired { get; }
        bool IsValueRequired { get; }
        bool MultiValues { get; }
        string Name { get; }
        string Type { get; }
        string Description { get; }
        int? Position { get; }

        bool TrySetValue(ICommand target, IEnumerable<string> value);
    }
}