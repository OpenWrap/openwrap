using System;
using System.Collections.Generic;

namespace OpenWrap.Commands.Remote
{
    public interface ISequenceBuilder : IEnumerable<ICommandOutput>
    {
        ISequenceBuilder Or(IEnumerable<ICommandOutput> returnValue);
        ISequenceBuilder Or(Func<ICommandOutput> returnValue);
    }
}