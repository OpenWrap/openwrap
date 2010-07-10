using System;
using System.Collections.Generic;

namespace OpenWrap.Commands.Remote
{
    public abstract class AbstractCommand : ICommand
    {
        public abstract IEnumerable<ICommandResult> Execute();

        protected ISequenceBuilder Either(IEnumerable<ICommandResult> returnValue)
        {
            return new SequenceBuilder(returnValue);
        }

        protected ISequenceBuilder Either(Func<ICommandResult> returnValue)
        {
            return new SequenceBuilder(returnValue.AsEnumerable());
        }
    }
}