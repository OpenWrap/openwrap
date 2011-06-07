using System;
using System.Net;

namespace OpenRasta.Client
{
    /// <summary>
    /// Just random code not part of anything yet.
    /// </summary>
    public interface IHttpClient
    {
        IClientRequest CreateRequest(Uri uri);
        Func<IWebProxy> Proxy { get; set; }
    }
}