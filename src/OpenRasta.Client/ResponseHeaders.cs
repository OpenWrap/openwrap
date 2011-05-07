using System;
using System.Collections.Generic;

namespace OpenRasta.Client
{
    public class ResponseHeaders : Dictionary<string, string>, IResponseHeaders
    {
        public ResponseHeaders() : base(StringComparer.OrdinalIgnoreCase)
        {
            
        }
        Uri _location;
        public Uri Location
        {
            get { return base.ContainsKey("Location") ? this["Location"].ToUri() : null; }
            set { _location = value; }
        }
    }
}