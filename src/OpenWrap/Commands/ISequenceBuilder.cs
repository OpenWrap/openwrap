using System;
using System.Collections.Generic;

namespace OpenWrap.Commands.Remote
{
    public interface ISequenceBuilder : IEnumerable<ICommandResult>
    {
        ISequenceBuilder Or(IEnumerable<ICommandResult> returnValue);
        ISequenceBuilder Or(Func<ICommandResult> returnValue);
    }
}