using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenWrap
{
    public static class Lazy
    {
        public static LazyValue<TReturn> Is<TReturn>(Func<TReturn> factory)
        {
            return new LazyValue<TReturn>(factory);
        }
    }
    public class LazyValue<T>
    {
        readonly Func<T> _factory;
        T _value;
        bool _valueBuilt = false;

        public LazyValue(Func<T> factory)
        {
            _factory = factory;
        }
        public static implicit operator T(LazyValue<T> lazy)
        {
            if (lazy._valueBuilt)
                return lazy._value;
            return lazy._value = lazy._factory();
        }
    }
}
