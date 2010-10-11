using ICSharpCode.SharpZipLib.Zip;
using OpenFileSystem.IO.FileSystem.Local;

namespace OpenFileSystem.IO
{
    public static class ZipExtensions
    {
        public static IFile ToZip(this IDirectory directory, Path path)
        {
            new FastZip().CreateZip(path.FullPath, directory.Path.FullPath, true, string.Empty);
            return directory.FileSystem.GetFile(path.FullPath);
        }
    }
}