using System;
using System.Net.Mime;

namespace OpenRasta.Client
{

    public class MediaType : ContentType
    {
        public MediaType(string mediaType)
            : base(mediaType)
        {
        }
    }
}