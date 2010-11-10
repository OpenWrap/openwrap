using System.Collections.Generic;
using OpenWrap.Commands;
using OpenWrap.Services;

namespace OpenWrap.Commands
{
    public interface ICommandRepository : ICollection<ICommandDescriptor>, IService
    {
        IEnumerable<string> Nouns { get; }
        IEnumerable<string> Verbs { get; }
        ICommandDescriptor Get(string verb, string name);
    }
}