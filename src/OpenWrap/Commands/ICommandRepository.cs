using System.Collections.Generic;
using OpenWrap.Commands;
using OpenWrap.Build.Services;

namespace OpenWrap.Commands
{
    public interface ICommandRepository : ICollection<ICommandDescriptor>, IService
    {
        IEnumerable<string> Namespaces { get; }
        IEnumerable<string> Verbs { get; }
        ICommandDescriptor Get(string @namespace, string name);
    }
}