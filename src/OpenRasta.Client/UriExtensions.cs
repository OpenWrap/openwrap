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
        public static Uri EnsureTrailingSlash(this Uri uri)
        {
            return uri.Segments.Length > 0 && uri.Segments.Last().Last() != '/'
                ? new Uri(uri.ToString() + "/")
                : uri;
        }
        public static Uri Combine(this Uri baseUri, Uri childUri)
        {
            return new Uri(baseUri, childUri);
        }
        public static Uri ToUri(this string value)
        {
            Uri parsedUri;
            try
            {
                parsedUri = new Uri(value, UriKind.RelativeOrAbsolute);
            }
            catch
            {
                parsedUri = null;
            }
            return parsedUri;
        }
    }
}
