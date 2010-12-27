using System;
using System.Collections.Generic;
using OpenWrap.Collections;

namespace OpenWrap.Commands
{
    public abstract class AbstractCommand : ICommand
    {
        public abstract IEnumerable<ICommandOutput> Execute();

        protected ISequenceBuilder Either(IEnumerable<ICommandOutput> returnValue)
        {
            return new SequenceBuilder(returnValue);
        }

        protected ISequenceBuilder AnyErrorBreaks(IEnumerable<ICommandOutput> returnValue)
        {
            return new SequenceBuilder(returnValue, false);
        }

        protected ISequenceBuilder Either(Func<ICommandOutput> returnValue)
        {
            return new SequenceBuilder(returnValue.AsEnumerable());
        }
    }
}