using System;

namespace OpenWrap
{
    public class LazyValue<T>
    {
        readonly Func<T> _factory;
        T _value;
        bool _valueBuilt;
        object _initLock;

        public LazyValue(Func<T> factory, bool takeLock = false)
        {
            _factory = factory;
            _initLock = takeLock ? new object() : null;
        }
        public T Value { get { return (T)this; } }
        public static implicit operator T(LazyValue<T> lazy)
        {
            if (lazy._valueBuilt)
                return lazy._value;
            if (lazy._initLock == null)
                return CreateValue(lazy);
            
            lock(lazy._initLock)
            {
                if (lazy._valueBuilt)
                    return lazy._value;
                else
                    return CreateValue(lazy);
            }
        }

        static T CreateValue(LazyValue<T> lazy)
        {
            lazy._value = lazy._factory();
            lazy._valueBuilt = true;
            return lazy._value;
        }
    }
}