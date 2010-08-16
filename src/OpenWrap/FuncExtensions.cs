using System;
using System.Collections.Generic;

namespace OpenWrap.Commands.Remote
{
    public static class FuncExtensions
    {
        /// <summary>
        /// Provides a delayed execution enumerable over a lambda.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IEnumerable<T> AsEnumerable<T>(this Func<T> value)
        {
            var resolvedValue = value();
            if (!ReferenceEquals(resolvedValue, null))
                yield return resolvedValue;
        }
        public static IEnumerable<T> AsEnumerable<T>(this Func<IEnumerable<T>> value)
        {
            foreach(var entry in value())
                yield return entry;
        }
    }
}