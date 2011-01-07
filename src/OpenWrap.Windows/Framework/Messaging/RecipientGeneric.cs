using System;

namespace OpenWrap.Windows.Framework.Messaging
{
    internal class RecipientGeneric<T> : Recipient
    {
        private readonly Action<T> _action;
        public RecipientGeneric(object target, Action<T> action) : base(target, null)
        {
            _action = action;
        }

        public void Send(T value)
        {
            _action(value);
        }
    }
}
