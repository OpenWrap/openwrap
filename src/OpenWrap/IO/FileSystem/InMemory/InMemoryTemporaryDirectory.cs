namespace OpenWrap.IO.FileSystem.InMemory
{
    public class InMemoryTemporaryDirectory : InMemoryDirectory, ITemporaryDirectory
    {
        public InMemoryTemporaryDirectory(string path) : base(path)
        {
        }

        public void Dispose()
        {
            Delete();
        }
    }
}