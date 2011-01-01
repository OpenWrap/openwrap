using System;

namespace OpenWrap.Windows.Framework.Messaging
{
    internal class Recipient
    {
        private readonly WeakReference _target;
        private readonly Action _action;

        public Recipient(object target, Action action)
        {
            _target = new WeakReference(target);
            _action = action;
        }

        public object Target
        {
            get { return _target.Target;  }
        }

        public bool IsAlive
        {
            get
            {
                if (_target == null)
                {
                    return false;
                }

                return _target.IsAlive;
            }
        }

        public void Send()
        {
            if (_action != null)
            {
                _action();
            }
        }
    }
}
