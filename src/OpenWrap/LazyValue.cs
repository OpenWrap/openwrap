using System;

namespace OpenWrap
{
    public class LazyValue<T>
    {
        readonly Func<T> _factory;
        T _value;
        bool _valueBuilt;

        public LazyValue(Func<T> factory)
        {
            _factory = factory;
        }

        public static implicit operator T(LazyValue<T> lazy)
        {
            if (lazy._valueBuilt)
                return lazy._value;
            lazy._value = lazy._factory();
            lazy._valueBuilt = true;
            return lazy._value;
        }
    }
}