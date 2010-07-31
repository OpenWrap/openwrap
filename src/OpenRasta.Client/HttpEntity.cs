using System.IO;

namespace OpenRasta.Client
{
    public class HttpEntity : IEntity
    {
        public HttpEntity(Stream stream)
        {
            Stream = stream;
            ContentType = new MediaType("application/octet-stream");
        }

        public Stream Stream { get; set; }

        public MediaType ContentType { get; set; }
    }
}