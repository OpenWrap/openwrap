using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Assemblies;
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
        static readonly Regex _openwrapRegex = new Regex(@"^openwrap-(?<version>\d+\.\d+\.\d+)$");
        static string _rootPath;
        static FileInfo _currentExecutable;
        static string _wrapsPath;
        static string _cachePath;

        static Program()
        {
            _rootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenWrap");
            _wrapsPath = Path.Combine(_rootPath, "wraps");
            _cachePath = Path.Combine(_wrapsPath, "cache");
        }
        static DirectoryInfo GetCacheDirectory(DirectoryInfo directory)
        {
            try
            {
                return directory.GetDirectories("wraps\\cache", SearchOption.TopDirectoryOnly).FirstOrDefault();
            }
            catch (IOException)
            {
                return null;
            }
        }

        static string GetProjectWrapAssembly()
        {
            var folder = from directory in GetSelfAndParents(Environment.CurrentDirectory)
                         where directory.Exists
                         let wrapDirectory = GetCacheDirectory(directory)
                         where wrapDirectory != null && wrapDirectory.Exists
                         from uncompressedFolder in wrapDirectory.GetDirectories()
                         let match = MatchFolderName(uncompressedFolder)
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

        static string GetSystemWrapAssembly()
        {
            var folder = from uncompressedFolder in UncompressedUserDirectories()
                         let match = _openwrapRegex.Match(uncompressedFolder.Name)
                         where match.Success
                         let version = new Version(match.Groups["version"].Value)
                         select new { uncompressedFolder, version };
            return folder.OrderBy(x => x.version).Select(x => x.uncompressedFolder.FullName).FirstOrDefault();
        }

        static int Main(string[] args)
        {
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
                var projectAssembly = GetProjectWrapAssembly() ??
                                      GetSystemWrapAssembly() ??
                                      TryDownloadOpenWrap();

                if (projectAssembly == null)
                    throw new EntryPointNotFoundException("Could not find OpenWrap assemblies in either current project or system repository.");

                string assemblyFile = Path.Combine(Path.Combine(projectAssembly, "bin-net35"), "OpenWrap.dll");
                var assembly = Assembly.LoadFrom(assemblyFile);
                System.Console.WriteLine("# OpenWrap v{0} ['{1}']", assembly.GetName().Version, projectAssembly);

                var type = assembly.GetType("OpenWrap.ConsoleRunner");

                var method = type.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);

                return (int)method.Invoke(null, new object[] { args });
            }
            catch (Exception e)
            {
                System.Console.WriteLine("OpenWrap console code not found.");
                System.Console.WriteLine(e.ToString());

                return -1;
            }
        }

        static string TryDownloadOpenWrap()
        {
            EnsureExists(_wrapsPath);
            System.Console.WriteLine("OpenWrap packages not found. Attempting download.");
            var client = new ConsoleProgressWebClient();

            var packagesToDownload = client.DownloadString(new Uri("http://wraps.openwrap.org/bootstrap", UriKind.Absolute))
                    .Split(new[] { "\\r\\n" }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(x=>!x.StartsWith("#"))
                    .Select(x=>new Uri(x, UriKind.Absolute));

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

                using(var wrapFileStream = File.OpenRead(wrapFilePath))
                    ZipArchive.Extract(wrapFileStream, extractFolder);
            }
            return GetSystemWrapAssembly();
        }

        static void EnsureExists(string wrapsPath)
        {
            if (!Directory.Exists(wrapsPath))
            {
                Directory.CreateDirectory(wrapsPath);
            }
        }

        public class ConsoleProgressWebClient
        {
            int _progress = 0;
            WebClient _webClient = new WebClient();
            ManualResetEvent _completed = new ManualResetEvent(false);
            Exception _error;
            string _stringReadResult;

            public ConsoleProgressWebClient()
            {
                _webClient.DownloadFileCompleted += DownloadFileCompleted;
                _webClient.DownloadStringCompleted += DownloadStringCompleted;
                _webClient.DownloadProgressChanged += DownloadProgressChanged;
            }
            public string DownloadString(Uri uri)
            {

                System.Console.Write("Downloading {0} [", uri);
                _webClient.DownloadStringAsync(uri);
                Wait();
                return _stringReadResult;
            }
            public void DownloadFile(Uri uri, string destinationFile)
            {

                System.Console.Write("Downloading {0} [", uri);
                _webClient.DownloadFileAsync(uri,destinationFile);
                Wait();
            }

            void DownloadProgressChanged(object src, DownloadProgressChangedEventArgs e)
            {
                var progress = e.ProgressPercentage / 10;
                //Debugger.Launch();
                if (_progress < progress && progress <= 10)
                {
                    System.Console.Write(new string('.', progress-_progress));
                    _progress = progress;
                }
            }

            void DownloadStringCompleted(object src, DownloadStringCompletedEventArgs e)
            {
                _stringReadResult = e.Result;
                Completed(e);
            }

            void DownloadFileCompleted(object src, AsyncCompletedEventArgs e)
            {
                Completed(e);
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

        static void AddPathToEnvironment(string openWrapRootPath)
        {
            var env = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
            if (env != null && env.Contains(openWrapRootPath))
                return;
            Environment.SetEnvironmentVariable("PATH", env + ";" + openWrapRootPath, EnvironmentVariableTarget.User);
            System.Console.WriteLine("Added '{0}' to PATH.", openWrapRootPath);
        }

        static Match MatchFolderName(DirectoryInfo uncompressedFolder)
        {
            return _openwrapRegex.Match(uncompressedFolder.Name);
        }

        static IEnumerable<DirectoryInfo> UncompressedUserDirectories()
        {
            var di = new DirectoryInfo(Path.Combine(_wrapsPath, "cache"));
            if (di.Exists)
                return di.GetDirectories();
            return Enumerable.Empty<DirectoryInfo>();
        }
    }
}