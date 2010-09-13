using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using TinySharpZip;

namespace OpenWrap.Console
{
    internal class Program
    {
        static readonly string _cachePath;
        static readonly Regex _openwrapRegex = new Regex(@"^(?<name>openwrap|openfilesystem)-(?<version>\d+(\.\d+(\.\d+(\.\d+)?)?)?)$", RegexOptions.IgnoreCase);
        static readonly string _rootPath;
        static readonly string _wrapsPath;
        static FileInfo _currentExecutable;

        static Program()
        {
            _rootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenWrap");
            _wrapsPath = Path.Combine(_rootPath, "wraps");
            _cachePath = Path.Combine(_wrapsPath, "_cache");
        }

        static void AddPathToEnvironment(string openWrapRootPath)
        {
            var env = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
            if (env != null && env.Contains(openWrapRootPath))
                return;
            Environment.SetEnvironmentVariable("PATH", env + ";" + openWrapRootPath, EnvironmentVariableTarget.User);
            System.Console.WriteLine("Added '{0}' to PATH.", openWrapRootPath);
        }

        static void EnsureExists(string wrapsPath)
        {
            if (!Directory.Exists(wrapsPath))
            {
                Directory.CreateDirectory(wrapsPath);
            }
        }

        static DirectoryInfo GetCacheDirectory(DirectoryInfo directory)
        {
            try
            {
                if (directory == null) return null;
                var cacheDirectory = new DirectoryInfo(Path.Combine(Path.Combine(directory.FullName, "wraps"), "_cache"));
                if (cacheDirectory.Exists)
                    return cacheDirectory;
                return null;
            }
            catch (IOException)
            {
                return null;
            }
        }

        static IEnumerable<string> GetProjectWrapAssembly()
        {
            return
                    (
                            from directory in GetSelfAndParents(Environment.CurrentDirectory)
                            where directory.Exists
                            let wrapDirectory = GetCacheDirectory(directory)
                            where wrapDirectory != null && wrapDirectory.Exists
                            from uncompressedFolder in wrapDirectory.GetDirectories()
                            let match = MatchFolderName(uncompressedFolder)
                            where match.Success
                            let version = new Version(match.Groups["version"].Value)
                            let name = match.Groups["name"]
                            group new { name, uncompressedFolder, version } by name
                            into tuplesByName
                            select tuplesByName.OrderByDescending(x => x.version).First().uncompressedFolder.FullName
                    ).ToList();
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

        static IEnumerable<string> GetSystemWrapAssembly()
        {
            return (
                           from uncompressedFolder in UncompressedUserDirectories()
                           let match = MatchFolderName(uncompressedFolder)
                           where match.Success
                           let version = new Version(match.Groups["version"].Value)
                           let name = match.Groups["name"]
                           group new { name, folder = uncompressedFolder.FullName, version } by name
                           into tuplesByName
                           select tuplesByName.OrderByDescending(x => x.version).First().folder
                   ).ToList();
        }

        static void InstallCurrentPath()
        {
            var path = _currentExecutable;
            if (!path.Exists)
                throw new FileNotFoundException("The console executable is not on a local file system.");

            var linkContent = Encoding.UTF8.GetBytes(_rootPath);
            using (var file = File.Create(Path.Combine(_rootPath, "o.exe.link")))
                file.Write(linkContent, 0, linkContent.Length);
            AddPathToEnvironment(path.Directory.FullName);
        }

        static void InstallFreshVersion()
        {
            System.Console.WriteLine("The OpenWrap shell is not installed on this machine. Do you want to:");
            System.Console.WriteLine("(i) install the shell and make it available on the path?");
            System.Console.WriteLine("(c) use the current executable location and make it available on the path?");
            System.Console.WriteLine("(n) do nothing?");
            var key = System.Console.ReadKey();
            System.Console.WriteLine();
            switch (key.KeyChar)
            {
                case 'i':
                case 'I':
                    InstallToDefaultLocation();
                    break;
                case 'c':
                case 'C':
                    InstallCurrentPath();
                    break;
            }
        }

        static void InstallToDefaultLocation()
        {
            var file = _currentExecutable;
            if (!file.Exists || file.Name != "o.exe")
                throw new FileNotFoundException("Couldn't find the console executable o.exe.");

            System.Console.WriteLine("Installing the shell to '{0}'.", _rootPath);
            if (!Directory.Exists(_rootPath))
                Directory.CreateDirectory(_rootPath);
            file.CopyTo(Path.Combine(_rootPath, "o.exe"));

            AddPathToEnvironment(_rootPath);
        }

        static int Main(string[] args)
        {
            if (args.Contains("-debug", StringComparer.OrdinalIgnoreCase))
            {
                Debugger.Launch();
                args = args.Where(x => x.IndexOf("-debug", StringComparison.OrdinalIgnoreCase) == -1).ToArray();
            }
            try
            {
                _currentExecutable = new FileInfo(typeof(Program).Assembly.Location);
                VerifyConsoleInstalled();
            }
            catch (Exception e)
            {
                System.Console.WriteLine("OpenWrap bootstrapping failed.");
                System.Console.WriteLine(e.ToString());
                return -100;
            }
            try
            {
                var bootstrapAssemblies = GetProjectWrapAssembly();
                if (bootstrapAssemblies.Count() == 0)
                    bootstrapAssemblies = GetSystemWrapAssembly();
                if (bootstrapAssemblies.Count() == 0)
                    bootstrapAssemblies = TryDownloadOpenWrap();

                if (bootstrapAssemblies.Count() == 0)
                    throw new EntryPointNotFoundException("Could not find OpenWrap assemblies in either current project or system repository.");

                var assemblyFiles = from asm in bootstrapAssemblies
                                    let path = Path.Combine(asm, "bin-net35")
                                    from file in Directory.GetFiles(path, "*.dll")
                                    select new { file, assembly = TryLoadAssembly(file) };
                ;

                var assembly = assemblyFiles.First();
                System.Console.WriteLine("# OpenWrap v{0} ['{1}']", assembly.assembly.GetName().Version, assembly.file);

                var type = assemblyFiles.Select(x=>x.assembly.GetType("OpenWrap.ConsoleRunner", false)).Where(x=>x != null).First();

                var method = type.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);

                return (int)method.Invoke(null, new object[] { args });
            }
            catch (Exception e)
            {
                System.Console.WriteLine("OpenWrap could not be started.");
                System.Console.WriteLine(e.Message);
                var oldColor = System.Console.ForegroundColor;
                try
                {
                    System.Console.ForegroundColor = ConsoleColor.Gray;
                    System.Console.WriteLine(e.ToString());
                }
                finally
                {
                    System.Console.ForegroundColor = oldColor;
                }
                return -1;
            }
        }

        static Assembly TryLoadAssembly(string asm)
        {
            return Assembly.LoadFrom(asm);
        }

        static Match MatchFolderName(DirectoryInfo uncompressedFolder)
        {
            return _openwrapRegex.Match(uncompressedFolder.Name);
        }

        static IEnumerable<string> TryDownloadOpenWrap()
        {
            EnsureExists(_wrapsPath);
            System.Console.WriteLine("OpenWrap packages not found. Attempting download.");
            var client = new ConsoleProgressWebClient();

            var packagesToDownload = client.DownloadString(new Uri("http://wraps.openwrap.org/bootstrap", UriKind.Absolute))
                    .Split(new[] { "\\r\\n" }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(x => !x.StartsWith("#"))
                    .Select(x => new Uri(x, UriKind.Absolute));

            foreach (var packageUri in packagesToDownload)
            {
                var packageClient = new ConsoleProgressWebClient();
                var fileName = packageUri.Segments.Last();
                string wrapFilePath = Path.Combine(_wrapsPath, fileName);

                if (File.Exists(wrapFilePath))
                    File.Delete(wrapFilePath);

                packageClient.DownloadFile(packageUri, wrapFilePath);

                System.Console.WriteLine("Expanding pacakge...");
                var extractFolder = Path.Combine(_cachePath, Path.GetFileNameWithoutExtension(fileName));

                if (Directory.Exists(extractFolder))
                    Directory.Delete(extractFolder, true);

                EnsureExists(extractFolder);

                using (var wrapFileStream = File.OpenRead(wrapFilePath))
                    ZipArchive.Extract(wrapFileStream, extractFolder);
            }
            return GetSystemWrapAssembly();
        }

        static void TryUpgrade(string consolePath)
        {
            var existingVersion = new Version(FileVersionInfo.GetVersionInfo(consolePath).FileVersion);
            var currentVersion = new Version(FileVersionInfo.GetVersionInfo(_currentExecutable.FullName).FileVersion);

            if (currentVersion > existingVersion)
            {
                System.Console.WriteLine("Upgrading '{0}' => '{1}'", existingVersion, currentVersion);
                File.Copy(_currentExecutable.FullName, consolePath, true);
            }
        }

        static IEnumerable<DirectoryInfo> UncompressedUserDirectories()
        {
            var di = new DirectoryInfo(Path.Combine(_wrapsPath, "_cache"));
            if (di.Exists)
                return di.GetDirectories();
            return Enumerable.Empty<DirectoryInfo>();
        }

        static void VerifyConsoleInstalled()
        {
            string oPath = Path.Combine(_rootPath, "o.exe");
            string linkPath = Path.Combine(_rootPath, "o.exe.link");
            if (!File.Exists(oPath))
            {
                if (!File.Exists(linkPath))
                    InstallFreshVersion();
                else
                    TryUpgrade(File.ReadAllText(linkPath, Encoding.UTF8));
            }
            else
            {
                TryUpgrade(oPath);
            }
        }

        public class ConsoleProgressWebClient
        {
            readonly ManualResetEvent _completed = new ManualResetEvent(false);
            readonly WebClient _webClient = new WebClient();
            Exception _error;
            int _progress;
            string _stringReadResult;

            public ConsoleProgressWebClient()
            {
                _webClient.DownloadFileCompleted += DownloadFileCompleted;
                _webClient.DownloadStringCompleted += DownloadStringCompleted;
                _webClient.DownloadProgressChanged += DownloadProgressChanged;
            }

            public void DownloadFile(Uri uri, string destinationFile)
            {
                System.Console.Write("Downloading {0} [", uri);
                _webClient.DownloadFileAsync(uri, destinationFile);
                Wait();
            }

            public string DownloadString(Uri uri)
            {
                System.Console.Write("Downloading {0} [", uri);
                _webClient.DownloadStringAsync(uri);
                Wait();
                return _stringReadResult;
            }

            void Completed(AsyncCompletedEventArgs e)
            {
                System.Console.WriteLine("]");
                if (e.Error != null)
                    _error = e.Error;
                _completed.Set();
            }

            void Wait()
            {
                _completed.WaitOne();
                if (_error != null)
                    throw new WebException("An error occured.", _error);
            }

            void DownloadFileCompleted(object src, AsyncCompletedEventArgs e)
            {
                Completed(e);
            }

            void DownloadProgressChanged(object src, DownloadProgressChangedEventArgs e)
            {
                var progress = e.ProgressPercentage / 10;

                if (_progress < progress && progress <= 10)
                {
                    System.Console.Write(new string('.', progress - _progress));
                    _progress = progress;
                }
            }

            void DownloadStringCompleted(object src, DownloadStringCompletedEventArgs e)
            {
                _stringReadResult = e.Result;
                Completed(e);
            }
        }
    }
}