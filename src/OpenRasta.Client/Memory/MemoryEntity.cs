using System.IO;
using OpenRasta.Client;

namespace Tests.Repositories.factories
{
    public class MemoryEntity : IEntity
    {
        public MemoryEntity(MediaType mediaType, Stream memoryStream)
        {
            Stream = memoryStream;
            ContentType = mediaType;
        }

        public Stream Stream { get; set; }
        public MediaType ContentType { get; set; }
    }
}