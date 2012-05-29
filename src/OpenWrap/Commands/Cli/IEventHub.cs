using System;

namespace OpenWrap.Commands.Cli
{
    public interface IEventHub
    {
        void Publish(object message);
        IDisposable Subscribe<T>(Action<T> handler);
    }
}