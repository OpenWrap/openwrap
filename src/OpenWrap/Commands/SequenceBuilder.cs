using System;
using System.Collections;
using System.Collections.Generic;
using OpenWrap.Commands.Remote;

namespace OpenWrap.Commands
{
    public class SequenceBuilder : ISequenceBuilder
    {
        readonly List<IEnumerable<ICommandOutput>> _results = new List<IEnumerable<ICommandOutput>>();

        public SequenceBuilder(IEnumerable<ICommandOutput> resultSets)
        {
            _results.Add(resultSets);
        }

        public ISequenceBuilder Or(IEnumerable<ICommandOutput> returnValue)
        {
            _results.Add(returnValue);
            return this;
        }
        public ISequenceBuilder Or(Func<ICommandOutput> returnValue)
        {
            _results.Add(returnValue.AsEnumerable());
            return this;
        }
        IEnumerator<ICommandOutput> IEnumerable<ICommandOutput>.GetEnumerator()
        {
            foreach(var resultSet in _results)
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