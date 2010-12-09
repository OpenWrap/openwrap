using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenWrap
{
    public static class Check
    {
        public static void NotNull(object value, string parameterName)
        {
            if (value == null)
                throw new ArgumentNullException(parameterName);
        }
        public static void NoNullElements<T>(IEnumerable<T> elements, string parameterName) where T:class
        {
            Check.NotNull(elements, parameterName);
            if (elements.Any(x => x == null))
                throw new ArgumentException(string.Format("One of the elements in {0} is null", parameterName), parameterName);
        }
    }
}
