using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenRasta.Client
{
    public static class UriExtensions
    {
        public static Uri BaseUri(this Uri address, Uri baseUri)
        {
            return address.IsAbsoluteUri ? address : new Uri(baseUri, address);
        }
    }
}
