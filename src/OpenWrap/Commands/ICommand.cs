using System.Collections.Generic;
using OpenWrap.Commands;

namespace OpenWrap.Commands
{
    public interface ICommand
    {
        IEnumerable<ICommandResult> Execute();
    }
}
