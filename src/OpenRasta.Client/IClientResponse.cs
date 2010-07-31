using System;

namespace OpenRasta.Client
{
    public interface IClientResponse : IResponse, IProgressNotification
    {
    }

    public interface IResponseHeaders
    {
        Uri Location { get; }
    }
    public class ResponseHeaders : IResponseHeaders
    {
        public Uri Location { get; set; }
    }
}