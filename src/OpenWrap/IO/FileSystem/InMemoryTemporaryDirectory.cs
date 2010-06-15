namespace OpenWrap.IO
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