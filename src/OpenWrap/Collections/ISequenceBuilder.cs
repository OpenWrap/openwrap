using System;
using System.Collections.Generic;
using OpenWrap.Commands;

namespace OpenWrap.Collections
{
    public interface ISequenceBuilder : IEnumerable<ICommandOutput>
    {
        ISequenceBuilder Or(IEnumerable<ICommandOutput> returnValue);
        ISequenceBuilder Or(Func<ICommandOutput> returnValue);
    }
}