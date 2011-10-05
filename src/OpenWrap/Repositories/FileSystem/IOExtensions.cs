using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenFileSystem.IO;

namespace OpenWrap.Repositories.FileSystem
{
    public static class IOExtensions
    {
        static readonly List<string> _drives = DriveInfo.GetDrives().Select(x => NormalizeToForwardSlash(x.RootDirectory.FullName).Replace("/", "")).ToList();

        public static OpenFileSystem.IO.Path ToPath(this Uri uri)
        {
            if (uri.Authority.EqualsNoCase("localhost") || string.IsNullOrEmpty(uri.Authority))
            {
                return new OpenFileSystem.IO.Path(uri.Segments.Skip(1)
                                                      .Select(_ => _.EndsWith("/") ? _.Substring(0, _.Length - 1) : _)
                                                      .JoinString(System.IO.Path.DirectorySeparatorChar));
            }
            return new OpenFileSystem.IO.Path(string.Format("{0}{0}{1}{2}",
                                                            System.IO.Path.DirectorySeparatorChar,
                                                            uri.Authority,
                                                            uri.Segments.Select(_ => _.EndsWith("/") ? _.Substring(0, _.Length - 1) : _).JoinString(System.IO.Path.DirectorySeparatorChar)));
        }

        public static Uri ToUri(this OpenFileSystem.IO.Path path, string scheme = "file")
        {
            // Lots of horrible things happening here due to OFS messing up badly with UNC paths <sigh>.
            var normalized = path.Segments.JoinString("/");

            string server = string.Empty;
            if (_drives.Contains(path.Segments.First()) == false)
            {
                server = path.Segments.First();
                normalized = path.Segments.Skip(1).JoinString("/");
            }
            return new Uri(string.Format("{0}://{1}/{2}", scheme, server, normalized), UriKind.Absolute);
        }

        static string NormalizeToForwardSlash(string path)
        {
            return path.Replace(System.IO.Path.DirectorySeparatorChar, '/')
                .Replace(System.IO.Path.AltDirectorySeparatorChar, '/');
        }

        public static bool SafeDelete(this IDirectory directory)
        {
            try
            {
                int count = 0;
                IDirectory deleteableDirectory;
                do
                {
                    deleteableDirectory = directory.Parent.GetDirectory("_" + directory.Name + "." + count++ + ".deleteme");
                } while (deleteableDirectory.Exists);

                directory.MoveTo(deleteableDirectory);
                deleteableDirectory.Delete();
            }
            catch (IOException)
            {
                return false;
            }
            return true;
        }
    }
}