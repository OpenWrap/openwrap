using System;
using System.Collections;
using System.Collections.Generic;
using OpenWrap.Commands;
using System.Linq;

namespace OpenWrap.Collections
{
    public class SequenceBuilder : ISequenceBuilder
    {
        readonly List<IEnumerable<ICommandOutput>> _results = new List<IEnumerable<ICommandOutput>>();

        public SequenceBuilder(IEnumerable<ICommandOutput> resultSets)
        {
            _results.Add(resultSets);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<ICommandOutput>)this).GetEnumerator();
        }

        IEnumerator<ICommandOutput> IEnumerable<ICommandOutput>.GetEnumerator()
        {
            return _results.SelectMany(co => co).TakeWhileIncluding(co => !(co is Error)).GetEnumerator();
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
    }
}