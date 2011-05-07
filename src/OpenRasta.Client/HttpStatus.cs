using System;

namespace OpenRasta.Client
{
    public class HttpStatus
    {
        public HttpStatus(int statusCode, string statusDescription)
        {
            Code = statusCode;
            Description = statusDescription;
        }

        public int Code { get; set; }
        public string Description { get; set; }
    }
}