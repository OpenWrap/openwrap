namespace OpenWrap.IO
{
    public static class FileSystem
    {
        static readonly IFileSystem _local = new LocalFileSystem();

        public static IFileSystem Local
        {
            get { return _local; }
        }
    }
}