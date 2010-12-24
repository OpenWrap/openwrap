using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace OpenWrap.Collections
{
    public class MultiThreadedEnumerable<T> : IEnumerable<T>
    {
        readonly AutoResetEvent _inputLock = new AutoResetEvent(false);

        public MultiThreadedEnumerable(IEnumerable<T> input1, IEnumerable<T> input2)
        {
            Inputs = new[] { input1, input2 };
        }

        public IEnumerable<T>[] Inputs { get; private set; }

        protected bool HasEntries
        {
            get { return false; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new MultiThreadedEnumerator<T>(this);
        }
    }
}