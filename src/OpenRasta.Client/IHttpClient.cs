using System;

namespace OpenRasta.Client
{
    /// <summary>
    /// Just random code not part of anything yet.
    /// </summary>
    public interface IHttpClient
    {
        IClientRequest CreateRequest(Uri uri);
        IHttpClient WithCredentials(string username, string password);
    }
}