using System;
using System.Collections;
using System.Collections.Generic;
using OpenWrap.Commands.Remote;

namespace OpenWrap.Commands
{
    public class SequenceBuilder : ISequenceBuilder
    {
        List<IEnumerable<ICommandOutput>> results = new List<IEnumerable<ICommandOutput>>();

        public SequenceBuilder(IEnumerable<ICommandOutput> resultSets)
        {
            results.Add(resultSets);
        }

        public ISequenceBuilder Or(IEnumerable<ICommandOutput> returnValue)
        {
            results.Add(returnValue);
            return this;
        }
        public ISequenceBuilder Or(Func<ICommandOutput> returnValue)
        {
            results.Add(returnValue.AsEnumerable());
            return this;
        }
        IEnumerator<ICommandOutput> IEnumerable<ICommandOutput>.GetEnumerator()
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
            return ((IEnumerable<ICommandOutput>)this).GetEnumerator();
        }
    }
}