using System.IO;

namespace OpenRasta.Client
{
    public interface IEntity
    {
        Stream Stream { get; set; }
        MediaType ContentType { get; set; }
    }
}