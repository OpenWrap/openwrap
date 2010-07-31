using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Security.Authentication.ExtendedProtection;
using System.Text;

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
