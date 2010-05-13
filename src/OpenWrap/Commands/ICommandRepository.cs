using System.Collections.Generic;
using OpenRasta.Wrap.Build.Services;
using OpenWrap.Commands;

namespace OpenRasta.Wrap.Console
{
    public interface ICommandRepository : ICollection<ICommandDescriptor>, IService
    {
        IEnumerable<string> Namespaces { get; }
        IEnumerable<string> Verbs { get; }
    }
}