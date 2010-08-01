using System;

namespace OpenRasta.Client
{
    public interface IRequest : IMessage
    {
        string Method { get; set; }
    }
}