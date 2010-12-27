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
        readonly Func<ICommandOutput, bool> _enumerationCondition;

        public SequenceBuilder(IEnumerable<ICommandOutput> resultSets) : this(resultSets, true)
        {
        }

        /// <summary>
        /// breakOnAny = true: Usual behaviour
        /// breakOnAny = false: Only error output will trigger a stop of the enumeration
        /// </summary>
        public SequenceBuilder(IEnumerable<ICommandOutput> resultSets, bool breakOnAny)
        {
            _results.Add(resultSets);
            _enumerationCondition = breakOnAny
                                            ? co => co == null
                                            : new Func<ICommandOutput, bool>(co => co.Type != CommandResultType.Error);

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<ICommandOutput>)this).GetEnumerator();
        }

        IEnumerator<ICommandOutput> IEnumerable<ICommandOutput>.GetEnumerator()
        {
            return _results.SelectMany(co => co).TakeWhileIncluding(_enumerationCondition).GetEnumerator();
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