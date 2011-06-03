using System.Collections.Generic;

namespace OpenWrap.Commands
{
    public interface ICommand
    {
        IEnumerable<ICommandOutput> Execute();
    }
}