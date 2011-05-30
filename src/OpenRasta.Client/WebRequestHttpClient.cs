using System;

namespace OpenRasta.Client
{
    public class WebRequestHttpClient : IHttpClient
    {

        readonly Func<Uri,IClientRequest> _requestCreator;

        public WebRequestHttpClient()
        {
            _requestCreator = uri => new HttpWebRequestBasedRequest(uri);
        }

        public IClientRequest CreateRequest(Uri uri)
        {
            return _requestCreator(uri);
        }
    }
    
}