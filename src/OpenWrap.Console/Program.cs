using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenWrap.Console
{
    internal class Program
    {
        static readonly Regex _openwrapRegex = new Regex(@"^openwrap-(?<version>\d+\.\d+\.\d+)$");
        static string _openWrapRootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenWrap");
        static FileInfo _currentExecutable;

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
            catch(Exception e)
            {
                System.Console.WriteLine("OpenWrap bootstrapping failed.");
                System.Console.WriteLine(e.ToString());
                return -100;
            }
            try
            {
                var projectAssembly = GetProjectWrapAssembly() ??
                                      GetSystemWrapAssembly();
                if (projectAssembly == null)
                    throw new EntryPointNotFoundException("Could not find OpenWrap assemblies in either current project or system repository.");
                string assemblyFile = Path.Combine(Path.Combine(projectAssembly, "bin-net35"), "OpenWrap.dll");
                var assembly = Assembly.LoadFrom(assemblyFile);
                System.Console.WriteLine("Using OpenWrap from {0}'.", assemblyFile);

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

        static void VerifyConsoleInstalled()
        {
            string oPath = Path.Combine(_openWrapRootPath, "o.exe");
            string linkPath = Path.Combine(_openWrapRootPath, "o.exe.link");
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
            AddPathToEnvironment(path.Directory.FullName);
        }

        static void InstallToDefaultLocation()
        {
            
            var file = _currentExecutable;
            if (!file.Exists || file.Name != "o.exe")
                throw new FileNotFoundException("Couldn't find the console executable o.exe.");

            file.CopyTo(Path.Combine(_openWrapRootPath, "o.exe"));

            AddPathToEnvironment(_openWrapRootPath);
        }

        static void AddPathToEnvironment(string openWrapRootPath)
        {
            var env = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
            if (env != null && env.Contains(openWrapRootPath))
                return;
            Environment.SetEnvironmentVariable("PATH", env + ";" + openWrapRootPath, EnvironmentVariableTarget.User);
            var path = Encoding.UTF8.GetBytes(openWrapRootPath);
            using(var file = File.Create(Path.Combine(_openWrapRootPath, "o.exe.link")))
                file.Write(path, 0, path.Length);
        }

        static Match MatchFolderName(DirectoryInfo uncompressedFolder)
        {
            return _openwrapRegex.Match(uncompressedFolder.Name);
        }

        static IEnumerable<DirectoryInfo> UncompressedUserDirectories()
        {
            var di = new DirectoryInfo(Path.Combine(Path.Combine(_openWrapRootPath, "wraps"), "cache"));
            if (di.Exists)
                return di.GetDirectories();
            return Enumerable.Empty<DirectoryInfo>();
        }
    }
}