using System;
using System.Collections.Generic;
using OpenWrap.Commands.Remote;

namespace OpenWrap.Commands
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