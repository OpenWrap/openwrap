using System;

namespace OpenRasta.Client
{
    public class HttpWebRequestBasedClient : IHttpClient
    {

        readonly Func<Uri,IClientRequest> _requestCreator;

        public HttpWebRequestBasedClient()
        {
            _requestCreator = uri => new HttpWebRequestBasedRequest(uri);
        }

        public IClientRequest CreateRequest(Uri uri)
        {
            return _requestCreator(uri);
        }
    }
    
}