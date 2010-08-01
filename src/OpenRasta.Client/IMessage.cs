using System;

namespace OpenRasta.Client
{
    public interface IMessage
    {
        IEntity Entity { get; }
        Uri RequestUri { get; }

    }
}