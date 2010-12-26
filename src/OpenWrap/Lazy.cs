using System;

namespace OpenWrap
{
    public static class Lazy
    {
        public static LazyValue<TReturn> Is<TReturn>(Func<TReturn> factory)
        {
            return new LazyValue<TReturn>(factory);
        }
    }
}