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

        private HttpWebRequestBasedClient(string username, string password)
        {
            _requestCreator = uri => new HttpWebRequestBasedRequest(uri, username, password);
        }

        public IClientRequest CreateRequest(Uri uri)
        {
            return _requestCreator(uri);
        }

        public IHttpClient WithCredentials(string username, string password)
        {
            return new HttpWebRequestBasedClient(username, password);
        }
    }
}