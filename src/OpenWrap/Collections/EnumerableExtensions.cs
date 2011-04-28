using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenWrap.Collections
{
    public static class EnumerableExtensions
    {
        public static T OneOrDefault<T>(this IEnumerable<T> input)
        {
            int count = 0;
            var enumerator = input.GetEnumerator();
            if (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                if (!enumerator.MoveNext()) return current;
            }
            return input.Where(_ => false).FirstOrDefault();
        }
        public static IEnumerable<T> Concat<T>(this IEnumerable<T> input, params T[] values)
        {
            return input.Concat((IEnumerable<T>)values);
        }

        public static bool Empty<T>(this IEnumerable<T> input)
        {
            return input.Any() == false;
        }

        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> input)
        {
            return input ?? Enumerable.Empty<T>();
        }

        public static IEnumerable<T> Merge<T>(this IEnumerable<T> input1, IEnumerable<T> input2)
        {
            return new MultiThreadedEnumerable<T>(input1, input2);
        }

        public static bool None<T>(this IEnumerable<T> input, Func<T, bool> condition) where T : class
        {
            return !input.Any(condition);
        }

        public static IEnumerable<T> NotNull<T>(this IEnumerable<T> input) where T : class
        {
            return input.Where(x => x != null);
        }

        public static IEnumerable<string> NotNullOrEmpty(this IEnumerable<string> input)
        {
            return input.Where(x => !string.IsNullOrEmpty(x));
        }

        public static IEnumerable<T> ToEnumerable<T>(this T item)
        {
            return new[] { item };
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

        public static IEnumerable<T> TakeWhileIncluding<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            var breakConditionMet = false;
            return items.TakeWhile(i =>
            {
                if (breakConditionMet) return false;
                if (!predicate(i))
                {
                    breakConditionMet = true;
                    return true;
                }
                return !breakConditionMet;
            });
        }
    }
}