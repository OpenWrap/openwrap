using System;

namespace OpenRasta.Client
{
    public class HttpWebRequestBasedClient : IHttpClient
    {
        public IClientRequest CreateRequest(Uri uri)
        {
            return new HttpWebRequestBasedRequest(uri);
        }
    }
}