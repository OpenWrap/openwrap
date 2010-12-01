using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace OpenWrap
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> input)
        {
            return input ?? Enumerable.Empty<T>();
        }
        public static IEnumerable<T> Concat<T>(this IEnumerable<T> input, params T[] values)
        {
            return input.Concat((IEnumerable<T>)values);
        }

        public static IEnumerable<T> Merge<T>(this IEnumerable<T> input1, IEnumerable<T> input2)
        {
            return new MultiThreadedEnumerable<T>(input1, input2);
        }

        public static bool None<T>(this IEnumerable<T> input, Func<T, bool> condition) where T : class
        {
            return !input.Any(condition);
        }
        public static bool Empty<T>(this IEnumerable<T> input)
        {
            return input.Any() == false;
        }
        public static IEnumerable<string> NotNullOrEmpty(this IEnumerable<string> input)
        {
            return input.Where(x => !string.IsNullOrEmpty(x));
        }
        public static IEnumerable<T> NotNull<T>(this IEnumerable<T> input) where T : class
        {
            return input.Where(x => x != null);
        }

        public static MoveNextResult TryMoveNext<T, TException>(this IEnumerator<T> enumerator, out T value, out TException error)
                where TException : Exception
        {
            value = default(T);
            error = default(TException);

            try
            {
                if (enumerator.MoveNext())
                {
                    value = enumerator.Current;
                    return MoveNextResult.Moved;
                }
                return MoveNextResult.End;
            }
            catch (TException e)
            {
                error = e;
                return MoveNextResult.Error;
            }
        }
    }

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