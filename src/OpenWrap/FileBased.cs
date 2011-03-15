using OpenFileSystem.IO;
using OpenWrap.PackageModel;

namespace OpenWrap
{
    public class FileBased<T> where T:class
    {
        public FileBased(IFile file, T value)
        {
            File = file;
            Value = value;
        }

        public IFile File { get; private set; }
        public T Value { get; private set; }
        public static implicit operator T(FileBased<T> fileBased)
        {
            return fileBased.Value;
        }
    }
    public static class FileBased
    {
        public static FileBased<T> New<T>(IFile file, T value) where T : class
        {
            return new FileBased<T>(file, value);
        }
    }
}