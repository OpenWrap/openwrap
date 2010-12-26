using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using OpenFileSystem.IO;
using Path = OpenFileSystem.IO.Path;

namespace OpenWrap.IO
{
    public static class ZipExtensions
    {
        public static IFile ToZip(this IDirectory directory, Path path)
        {
            new FastZip().CreateZip(path.FullPath, directory.Path.FullPath, true, string.Empty);
            return directory.FileSystem.GetFile(path.FullPath);
        }
        public static T Read<T>(this ZipFile file, ZipEntry zipEntry, Func<Stream, T> read)
        {
            return Read(() => file.GetInputStream(zipEntry), read);
        }
        static T Read<T>(Func<Stream> open, Func<Stream, T> read)
        {
            using (var stream = open())
                return read(stream);
        }

    }
}