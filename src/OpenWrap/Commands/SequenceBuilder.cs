using System;
using System.Collections;
using System.Collections.Generic;
using OpenWrap.Commands.Remote;

namespace OpenWrap.Commands
{
    public class SequenceBuilder : ISequenceBuilder
    {
        List<IEnumerable<ICommandResult>> results = new List<IEnumerable<ICommandResult>>();

        public SequenceBuilder(IEnumerable<ICommandResult> resultSets)
        {
            results.Add(resultSets);
        }

        public ISequenceBuilder Or(IEnumerable<ICommandResult> returnValue)
        {
            results.Add(returnValue);
            return this;
        }
        public ISequenceBuilder Or(Func<ICommandResult> returnValue)
        {
            results.Add(returnValue.AsEnumerable());
            return this;
        }
        IEnumerator<ICommandResult> IEnumerable<ICommandResult>.GetEnumerator()
        {
            foreach(var resultSet in results)
            {
                var hadValue = false;
                var enumerator = resultSet.GetEnumerator();
                while(enumerator.MoveNext())
                {
                    if (enumerator.Current == null)
                        continue;
                    hadValue = true;
                    yield return enumerator.Current;
                }
                if (hadValue)
                    yield break;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<ICommandResult>)this).GetEnumerator();
        }
    }
}