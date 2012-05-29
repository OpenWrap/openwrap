using System;
using System.Net;

namespace OpenRasta.Client
{
    public class WebRequestHttpClient : IHttpClient
    {
        readonly Func<Uri, WebProxy, IClientRequest> _requestCreator;

        public WebRequestHttpClient()
        {
            _requestCreator = (uri, proxy) =>
            {
                var request = new HttpWebRequestBasedRequest(uri, proxy)
                {

                };
                Notifier.RaiseNewRequest(request);
                return request;
            };
            Notifier = HttpNotifier.Default;
        }

        public HttpNotifier Notifier { get; set; }
        public Func<IWebProxy> Proxy { get; set; }

        Func<IWebProxy> CurrentProxy
        {
            get { return Proxy ?? WebRequest.GetSystemWebProxy; }
        }

        public IClientRequest CreateRequest(Uri uri)
        {
            var proxy = CurrentProxy();

            if (proxy != null && proxy.IsBypassed(uri) == false)
            {
                var finalProxy = new WebProxy(proxy.GetProxy(uri), false);
                if (proxy.Credentials != null)
                    finalProxy.Credentials = proxy.Credentials;
                else
                    finalProxy.UseDefaultCredentials = true;
                return _requestCreator(uri, finalProxy);
            }

            return _requestCreator(uri, null);
        }
    }
}