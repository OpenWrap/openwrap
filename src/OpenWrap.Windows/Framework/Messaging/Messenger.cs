using System;
using System.Collections.Generic;

namespace OpenWrap.Windows.Framework.Messaging
{
    /// <summary>
    /// messaging system is inspired by MVVMLight
    /// but with less stuff in it
    /// </summary>
    public class Messenger
    {
        private readonly Dictionary<string, Recipients> _allActions = new Dictionary<string, Recipients>();
        private static Messenger _instance;

        private Messenger()
        {
        }

        public static Messenger Default
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Messenger();
                }
                return _instance;
            }
        }

        public void Send(string key)
        {
            if (!_allActions.ContainsKey(key))
            {
                return;
            }

            Recipients recipients = _allActions[key];
            recipients.Send();
        }

        public void Send<T>(string key, T value)
        {
            if (! _allActions.ContainsKey(key))
            {
                return;
            }

            Recipients recipients = _allActions[key];
            recipients.Send(value);
        }

        public void Subcribe<T>(string key, object target, Action<T> action)
        {
            TestCreateListForKey(key);
            _allActions[key].Subcribe(target, action);
        }

        public void Subcribe(string key, object target, Action action)
        {
            TestCreateListForKey(key);
            _allActions[key].Subcribe(target, action);
        }

        public void Unsubcribe(string key, object target)
        {
            if (_allActions.ContainsKey(key))
            {
                _allActions[key].Unsubscribe(target);
            }
        }

        private void TestCreateListForKey(string key)
        {
            if (!_allActions.ContainsKey(key))
            {
                _allActions.Add(key, new Recipients());
            }
        }
    }
}
