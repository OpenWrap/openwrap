using System;
using System.Net;

namespace OpenRasta.Client
{
    public interface IRequest : IMessage
    {
        string Method { get; set; }
    }
}