using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;
using OpenFileSystem.IO;

namespace OpenWrap.IO
{
    public static class IOExtensions
    {
        public static T Read<T>(this IFile file, Func<Stream, T> read)
        {
            return Read(() => file.OpenRead(), read);
        }

        public static T ReadRetry<T>(this IFile file, Func<Stream, T> read, int retries = 10, int wait = 50)
        {
            for (int retry = 0; ; )
            {
                try
                {
                    return Read(() => file.OpenRead(), read);
                }
                catch
                {
                    if (retry++ > retries)
                        throw;

                    Thread.Sleep(wait);
                }
            }
        }

        public static IEnumerable<string> ReadLines(this IFile file)
        {
            return ReadLines(file, Encoding.UTF8);
        }

        public static IEnumerable<string> ReadLines(this IFile file, Encoding encoding)
        {
            using (var reader = file.OpenRead().StreamReader(encoding))
                while (!reader.EndOfStream)
                    yield return reader.ReadLine();
        }

        public static string ReadString(this IFile file)
        {
            return ReadString(file, Encoding.UTF8);
        }

        public static string ReadString(this IFile file, Encoding encoding)
        {
            using (var stream = file.OpenRead())
                return stream.ReadString(encoding);
        }

        public static bool TryDelete(this IDirectory directory)
        {
            try
            {
                var directoryToDelete = directory.Parent.GetDirectory(directory.Name + ".old");
                directory.MoveTo(directoryToDelete);
                directoryToDelete.Delete();
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        /// <summary>
        ///   Writes the provided string to a file, using the provided encoding. If the file already exists, it will be overwritten.
        /// </summary>
        /// <param name = "file"></param>
        /// <param name = "encoding"></param>
        /// <returns></returns>
        public static void WriteString(this IFile file, string text)
        {
            WriteString(file, text, Encoding.UTF8);
        }

        /// <summary>
        ///   Writes the provided string to a file, using the provided encoding. If the file already exists, it will be overwritten.
        /// </summary>
        /// <param name = "file"></param>
        /// <param name = "encoding"></param>
        /// <returns></returns>
        public static void WriteString(this IFile file, string text, Encoding encoding)
        {
            using (var stream = file.OpenWrite())
            {
                stream.WriteString(text, encoding);
            }
        }
        static T Read<T>(Func<Stream> open, Func<Stream, T> read)
        {
            using (var stream = open())
                return read(stream);
        }
    }
}