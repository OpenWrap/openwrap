using System;

namespace OpenWrap
{
    public static class Lazy
    {
        public static LazyValue<TReturn> Is<TReturn>(Func<TReturn> factory, bool takeLock = false)
        {
            return new LazyValue<TReturn>(factory, takeLock);
        }
    }
}