using System;
using System.Collections.Generic;
using System.Linq;

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
}