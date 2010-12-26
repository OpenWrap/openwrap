using System;
using System.Collections.Generic;

namespace OpenWrap
{
    public static class FuncExtensions
    {
        /// <summary>
        ///   Provides a delayed execution enumerable over a lambda.
        /// </summary>
        /// <typeparam name = "T"></typeparam>
        /// <param name = "value"></param>
        /// <returns></returns>
        public static IEnumerable<T> AsEnumerable<T>(this Func<T> value)
        {
            var resolvedValue = value();
            if (!ReferenceEquals(resolvedValue, null))
                yield return resolvedValue;
        }
    }
}