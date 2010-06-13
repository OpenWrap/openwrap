using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace OpenWrap.Console
{
    internal class Program
    {
        static int Main(string[] args)
        {
            try
            {
                var projectAssembly = GetProjectWrapAssembly() ??
                                      GetUserWrapAssembly();
                var assembly = Assembly.LoadFrom(Path.Combine(Path.Combine(projectAssembly, "bin-net35"), "OpenWrap.dll"));

                var type = assembly.GetType("OpenWrap.ConsoleRunner");
                
                var method = type.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);

                return (int)method.Invoke(null, new object[]{args});
            }
            catch
            {
                System.Console.WriteLine("OpenWrap not found. ");
                return -1;
            }
        }

        static string GetUserWrapAssembly()
        {
            var folder = from uncompressedFolder in UncompressedUserDirectories()
                         let match = _openwrapRegex.Match(uncompressedFolder.Name)
                         where match.Success
                         let version = new Version(match.Groups["version"].Value)
                         select new { uncompressedFolder, version };
            return folder.OrderBy(x => x.version).Select(x => x.uncompressedFolder.FullName).FirstOrDefault();
        }

        static IEnumerable<DirectoryInfo> UncompressedUserDirectories()
        {
            
            var di = new DirectoryInfo(Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "openwrap"), "wraps"), "cache"));
            if (di.Exists)
                return di.GetDirectories();
            return Enumerable.Empty<DirectoryInfo>();
        }

        static Regex _openwrapRegex = new Regex(@"openwrap-(?<version>\d+\.\d+\.\d+)");
        static string GetProjectWrapAssembly()
        {
            var folder = from directory in GetSelfAndParents(Environment.CurrentDirectory)
                         let wrapDirectory = directory.GetDirectories("wraps\\cache\\", SearchOption.TopDirectoryOnly).FirstOrDefault()
                         where wrapDirectory != null && wrapDirectory.Exists
                         from uncompressedFolder in wrapDirectory.GetDirectories()
                         let match = _openwrapRegex.Match(uncompressedFolder.Name)
                         where match.Success
                         let version = new Version(match.Groups["version"].Value)
                         select new { uncompressedFolder, version };

            return folder.OrderBy(x => x.version).Select(x => x.uncompressedFolder.FullName).FirstOrDefault();
        }

        static IEnumerable<DirectoryInfo> GetSelfAndParents(string directoryPath)
        {
            var directory = new DirectoryInfo(directoryPath);
            do
            {
                yield return directory;
                directory = directory.Parent;
            } while (directory != null);
            }
    }
}