using System;
using System.Collections.Generic;

namespace OpenWrap.Windows.Framework.Messaging
{
    internal class Recipients
    {
        private readonly List<Recipient> _recipients = new List<Recipient>();

        public void Send()
        {
            RemoveExpired();

            foreach (Recipient recipient in _recipients)
            {
                recipient.Send();
            }
        }

        public void Send<T>(T value)
        {
            RemoveExpired();

            foreach (Recipient recipient in _recipients)
            {
                RecipientGeneric<T> recipientGeneric = recipient as RecipientGeneric<T>;
                if (recipientGeneric != null)
                {
                    recipientGeneric.Send(value);
                }
            }
        }

        public void Subcribe<T>(object target, Action<T> action)
        {
            _recipients.Add(new RecipientGeneric<T>(target, action));
        }

        public void Subcribe(object target, Action action)
        {
            _recipients.Add(new Recipient(target, action));
        }

        public void Unsubscribe(object target)
        {
            _recipients.RemoveAll(rec => rec.Target == target);
        }

        public void RemoveExpired()
        {
            _recipients.RemoveAll(rec => !rec.IsAlive);
        }
    }
}
