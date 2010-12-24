using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace OpenWrap.Collections
{
    public class MultiThreadedEnumerator<T> : IEnumerator<T>
    {
        readonly Queue<T> _cachedItems = new Queue<T>(2);
        readonly IEnumerable<EnumerableEntry> _entries;
        readonly MultiThreadedEnumerable<T> _parent;

        public MultiThreadedEnumerator(MultiThreadedEnumerable<T> parent)
        {
            _parent = parent;
            _entries = parent.Inputs.Select(x => new EnumerableEntry(this, x)).ToList();
        }

        public Queue<T> CachedItems
        {
            get { return _cachedItems; }
        }

        public T Current { get; private set; }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            if (TryTakeCachedValue()) return true;
            return FeedNewValues();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        bool FeedNewValues()
        {
            var inputs = _entries.Where(x => !x.IsFinished).ToList();

            if (inputs.Count == 0)
                return false;
            var syncPrimitives = inputs.Select(x => x.Done).ToArray();

            foreach (var input in inputs)
                input.QueueRead();

            do
            {
                int notified = WaitHandle.WaitAny(syncPrimitives);

                if (TryTakeCachedValue()) return true;
            } while (_entries.Any(x => !x.IsFinished));

            return TryTakeCachedValue();
        }

        void QueueCachedValue(T current)
        {
            lock (CachedItems)
            {
                CachedItems.Enqueue(current);
            }
        }

        bool TryTakeCachedValue()
        {
            lock (CachedItems)
            {
                if (CachedItems.Count > 0)
                {
                    Current = CachedItems.Dequeue();
                    return true;
                }
            }
            return false;
        }

        public class EnumerableEntry
        {
            readonly MultiThreadedEnumerator<T> _parent;
            readonly object _syncRoot = new object();

            public EnumerableEntry(MultiThreadedEnumerator<T> parent, IEnumerable<T> entry)
            {
                _parent = parent;
                Entry = entry;
                this.Enumerator = entry.GetEnumerator();
                Done = new AutoResetEvent(false);
            }

            public AutoResetEvent Done { get; set; }
            public bool IsFinished { get; private set; }
            public int Pending { get; set; }
            protected IEnumerator<T> Enumerator { get; set; }
            IEnumerable<T> Entry { get; set; }

            public void QueueRead()
            {
                Pending++;
                ThreadPool.QueueUserWorkItem(x =>
                {
                    lock (_syncRoot)
                    {
                        var hasValue = Enumerator.MoveNext();

                        if (hasValue)
                        {
                            _parent.QueueCachedValue(Enumerator.Current);
                        }
                        else
                        {
                            IsFinished = true;
                        }
                        Done.Set();
                        Pending--;
                    }
                });
            }
        }
    }
}