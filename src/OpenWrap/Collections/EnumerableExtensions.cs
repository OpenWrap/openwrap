using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace OpenWrap
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> NotNull<T>(this IEnumerable<T> input) where T : class
        {
            return input.Where(x => x != null);
        }
        public static IEnumerable<T> Concat<T>(this IEnumerable<T> input, params T[] values)
        {
            return input.Concat((IEnumerable<T>)values);
        }
        public static bool None<T>(this IEnumerable<T> input, Func<T, bool> condition) where T : class
        {
            return input.Any(x => !condition(x));
        }
        public static IEnumerable<T> Merge<T>(this IEnumerable<T> input1, IEnumerable<T> input2)
        {
            return new MultiThreadedEnumerable<T>(input1, input2);
        }
        public static MoveNextResult TryMoveNext<T, TException>(this IEnumerator<T> enumerator, out T value, out TException error)
            where TException : Exception
        {
            value = default(T);
            error = default(TException);

            try
            {
                if(enumerator.MoveNext())
                {
                    value = enumerator.Current;
                    return MoveNextResult.Moved;
                }
                return MoveNextResult.End;
            }
            catch(TException e)
            {
                error = e;
                return MoveNextResult.Error;
            }
        }
    }

    public class MultiThreadedEnumerable<T> : IEnumerable<T>
    {
        readonly AutoResetEvent _inputLock = new AutoResetEvent(false);
        public IEnumerable<T>[] Inputs { get; private set; }

        public MultiThreadedEnumerable(IEnumerable<T> input1, IEnumerable<T> input2)
        {
            Inputs = new[] { input1, input2 };
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new MultiThreadedEnumerator<T>(this);
        }

        protected bool HasEntries { get { return false; } }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }

    public class MultiThreadedEnumerator<T> : IEnumerator<T>
    {
        readonly MultiThreadedEnumerable<T> _parent;
        Queue<T> _cachedItems = new Queue<T>(2);
        IEnumerable<EnumerableEntry<T>> _entries;
        public Queue<T> CachedItems { get { return _cachedItems; } }
        public MultiThreadedEnumerator(MultiThreadedEnumerable<T> parent)
        {
            _parent = parent;
            _entries = parent.Inputs.Select(x => new EnumerableEntry<T>(this, x)).ToList();
        }

        public void Dispose()
        {
            
        }

        public bool MoveNext()
        {
            if (TryTakeCachedValue()) return true;
            return FeedNewValues();
        }

        bool FeedNewValues()
        {
            var inputs = _entries.Where(x => !x.IsFinished).ToList();
            Console.WriteLine("Enumerator: {0}: Inputs count {1}", GetHashCode(), inputs.Count);

            if (inputs.Count == 0)
                return false;
            var syncPrimitives = inputs.Select(x => x.Done).ToArray();
            
            foreach (var input in inputs)
                input.QueueRead();
            Console.WriteLine("Enumerator: {0}: Waiting", GetHashCode());

            do
            {
                int notified = WaitHandle.WaitAny(syncPrimitives);

                //syncPrimitives[notified].Reset();

                if(TryTakeCachedValue()) return true;

            } while (_entries.Any(x=>!x.IsFinished));

            return TryTakeCachedValue();
        }

        bool TryTakeCachedValue()
        {
            lock(CachedItems)
            {
                if (CachedItems.Count > 0)
                {
                    Current = CachedItems.Dequeue();
                    Console.WriteLine("Enumerator: {0}: Cached value returned", GetHashCode());
                    
                    return true;
                }
            }
            Console.WriteLine("Enumerator: {0}: No cache value", GetHashCode());

            return false;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public T Current { get; private set; }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        void QueueCachedValue(T current)
        {
            lock(CachedItems)
            {
                CachedItems.Enqueue(current);
            }
        }

        public class EnumerableEntry<T>
        {
            readonly MultiThreadedEnumerator<T> _parent;
            public EnumerableEntry(MultiThreadedEnumerator<T> parent, IEnumerable<T> entry)
            {

                _parent = parent;
                Entry = entry;
                this.Enumerator = entry.GetEnumerator();
                Done = new AutoResetEvent(false);
            }

            protected IEnumerator<T> Enumerator { get; set; }
            public AutoResetEvent Done { get; set; }
            public int Pending { get; set; }
           public  bool IsFinished { get; private set; }
            IEnumerable<T> Entry { get; set; }

            public void QueueRead()
            {
                Console.WriteLine("Enumerable: {0}: QueueRead", GetHashCode());
                Pending++;
                ThreadPool.QueueUserWorkItem(x =>
                {
                    lock (Enumerator)
                    {
                        Console.WriteLine("Enumerable: {0}: MoveNext", GetHashCode());

                        var hasValue = Enumerator.MoveNext();
                        Console.WriteLine("Enumerable: {0}: MoveNext finsihed {1}", GetHashCode(), hasValue);

                        if (hasValue)
                        {
                            Console.WriteLine("Enumerable: {0}: QueueCachedValue {1}", GetHashCode(), Enumerator.Current);

                            _parent.QueueCachedValue(Enumerator.Current);
                        }
                        else
                        {
                            Console.WriteLine("Enumerable: {0}: Finished", GetHashCode());

                            IsFinished = true;
                        }
                        Console.WriteLine("Enumerable: {0}: Done.Set", GetHashCode());

                        Done.Set();
                        Pending--;

                    }
                });
            }
        }
    }
}