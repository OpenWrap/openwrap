using OpenWrap.IO.FileSystem.Local;

namespace OpenWrap.IO
{
    public static class FileSystems
    {
        static readonly IFileSystem _local = new LocalFileSystem();

        public static IFileSystem Local
        {
            get { return _local; }
        }
    }
}