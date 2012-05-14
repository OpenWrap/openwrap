using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using OpenFileSystem.IO;
using OpenWrap.Collections;
using OpenWrap.IO;
using OpenWrap.Runtime;
using Path = System.IO.Path;

namespace OpenWrap.Build.PackageBuilders
{
    public class MSBuildPackageBuilder : AbstractProcessPackageBuilder
    {
        readonly IEnvironment _environment;

        public MSBuildPackageBuilder(IFileSystem fileSystem, IEnvironment environment, IFileBuildResultParser parser)
            : base(fileSystem, parser)
        {
            _environment = environment;
            Profile = new List<string>();
            Platform = new List<string>();
            Project = new List<string>();
            Configuration = new List<string>();
            ProjectFiles = new List<IFile>();

            Properties = new string[0].ToLookup(_ => _);
        }

        public static readonly string[] ReservedProperties = new[] { "BuildEngine" };
        public ILookup<string, string> Properties { get; set; }

        public IEnumerable<string> Platform { get; set; }
        public IEnumerable<string> Profile { get; set; }
        public IEnumerable<string> Project { get; set; }
        public IEnumerable<string> Configuration { get; set; }
        public ICollection<IFile> ProjectFiles { get; set; }

        public string BuildEngine { get; set; }
        protected override string ExecutablePath
        {
            get { return GetExecutablePath(); }
        }

        public bool Incremental { get; set; }

        public override IEnumerable<BuildResult> Build()
        {
            var currentDirectory = _environment.CurrentDirectory;
            ProjectFiles.Clear();
            if (Project.Count() > 0)
            {
                foreach (var proj in Project)
                {
                    var pathSpec = _environment.DescriptorFile.Parent.Path.Combine(proj).FullPath;
                    IEnumerable<IFile> specFiles;
                    // TODO: Fix OFS. Horribe construction, the Files extension method seems to be buggy as fuck.
                    try
                    {
                        specFiles = _fileSystem.Files(pathSpec);
                    }
                    catch (Exception)
                    {
                        specFiles = GetWildcardFile(pathSpec);
                    }
                    if (specFiles.Any() == false)
                        specFiles = GetWildcardFile(pathSpec);

                    if (specFiles.Any() == false || specFiles.Any(x => x.Exists == false))
                    {
                        yield return new UnknownProjectFileResult(proj);
                        yield break;
                    }
                    ProjectFiles.AddRange(specFiles);
                }
            }
            else
            {
                var sourceDirectory = currentDirectory.GetDirectory("src");
                if (!sourceDirectory.Exists)
                {
                    yield return
                            new ErrorBuildResult(string.Format("Could not locate a /src folder in current directory '{0}'. Make sure you use the default layout for project code.",
                                                               _environment.CurrentDirectory.Path.FullPath));
                    yield break;
                }
                ProjectFiles.AddRange(sourceDirectory.Files("*.*proj", SearchScope.SubFolders));
            }

            var builds = from file in ProjectFiles
                         from platform in Platform.DefaultIfEmpty(null)
                         from profile in Profile.DefaultIfEmpty(null)
                         select new { file, platform, profile };

            yield return new TextBuildResult(string.Format("Using MSBuild from path '{0}'.", ExecutablePath));

            if (!Incremental)
                foreach (var project in builds)
                {
                    using (var responseFile = _fileSystem.CreateTempFile())
                    {
                        foreach (var value in ExecuteEngine(CreateMSBuildProcess(responseFile, project.file, project.platform, project.profile, "Clean")))
                            yield return value;
                    }
                }

            foreach (var project in builds)
            {
                using (var responseFile = _fileSystem.CreateTempFile())
                {
                    var msbuildProcess = CreateMSBuildProcess(responseFile, project.file, project.platform, project.profile, "Build");
                    yield return new InfoBuildResult(string.Format("Building '{0}'...", project.file.Path.FullPath));


                    foreach (var m in ExecuteEngine(msbuildProcess)) yield return m;
                }
            }
        }

        IEnumerable<IFile> GetWildcardFile(string pathSpec)
        {
            IEnumerable<IFile> specFiles = null;
            try
            {
                specFiles = new[] { _fileSystem.GetFile(pathSpec) };
            }
            catch { }
            return specFiles ?? Enumerable.Empty<IFile>();
        }
        string GetExecutablePath()
        {
            var build = Properties.Contains("BuildEngine") && Properties["BuildEngine"].Any() ? Properties["BuildEngine"].First() : BuildEngine;

            string path = null;
            if (build == "default" || build == "msbuild")
                path = GetMSBuildExecutionPath();
            if (build == "xbuild" || (build == "default" && path == null))
                path = IsUnix ? "xbuild.exe" : GetWinXbuildPath();
            return path;
        }

        static bool IsUnix
        {
            get 
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        static string GetWinXbuildPath()
        {
            const string monoRegKey = @"SOFTWARE\Novell\Mono";
            const string monoRegKeyx64 = @"SOFTWARE\Wow6432Node\Novell\Mono";
            var installKey = Registry.LocalMachine.OpenSubKey(monoRegKey) ??
                             Registry.LocalMachine.OpenSubKey(monoRegKeyx64);
            if (installKey == null) return null;
            var defaultValue = installKey.GetValue("DefaultCLR");

            var versionedKey = installKey.OpenSubKey(defaultValue.ToString());
            if (versionedKey == null) return null;

            var libPath = versionedKey.GetValue("FrameworkAssemblyDirectory").ToString();
            return Path.Combine(libPath, "mono\\4.0\\xbuild.exe");
        }

        static string GetMSBuildExecutionPath()
        {
            return (from versionFolder in Directory.GetDirectories(Environment.ExpandEnvironmentVariables(@"%windir%\Microsoft.NET\Framework\"), "v*")
                    let version = new Version(Path.GetFileName(versionFolder).Substring(1))
                    orderby version descending
                    let file = Path.Combine(versionFolder, "msbuild.exe")
                    where File.Exists(file)
                    select file).FirstOrDefault();
        }

        IProcess CreateMSBuildProcess(IFile responseFile, IFile project, string platform, string profile, string msbuildTargets)
        {

            using (var stream = responseFile.OpenWrite())
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                var existingResponseFile = Environment.GetCommandLineArgs()
                        .Select(x => x.Trim())
                        .Where(x => x.StartsWith("@"))
                        .Select(x => _fileSystem.GetFile(x.Substring(1)))
                        .FirstOrDefault(x => x.Exists);
                if (existingResponseFile != null && existingResponseFile.Exists)
                {
                    foreach (var line in existingResponseFile.ReadLines().Where(NotTarget))
                        writer.WriteLine(line);
                }
                if (platform != null) writer.WriteLine("/p:OpenWrap-TargetPlatform=" + platform);
                if (profile != null)
                {
                    var msbuildVersioning = TargetFramework.ParseOpenWrapIdentifier(profile);
                    writer.WriteLine("/p:OpenWrap-TargetProfile=" + profile);
                    writer.WriteLine("/p:TargetFrameworkVersion=" + msbuildVersioning.Version);
                    writer.WriteLine("/p:TargetFrameworkIdentifier=" + msbuildVersioning.Identifier);
                    writer.WriteLine("/p:TargetFrameworkProfile=" + msbuildVersioning.Profile);
                }
                if (Configuration.Any())
                {
                    writer.WriteLine("/p:Configuration=" + Configuration.Last());
                }
                if (Debugger.IsAttached)
                    writer.WriteLine("/p:OpenWrap-StartDebug=true");
                writer.WriteLine("/p:OpenWrap-EmitOutputInstructions=true");
                writer.WriteLine("/p:OpenWrap-CurrentProjectFile=\"" + project.Path.FullPath + "\"");

                foreach (var kv in Properties.Where(_=>ReservedProperties.Contains(_.Key) == false))
                {
                    var value = kv.LastOrDefault();
                    if (value != null)
                        writer.WriteLine("/p:{0}={1}", kv.Key, value);
                }
                if (msbuildTargets != null)
                    writer.WriteLine("/t:" + msbuildTargets);

                var args = string.Format(@"@""{1}"" ""{0}""", project.Path.FullPath, responseFile.Path.FullPath);
                var msBuildProcess = CreateProcess(args);
                CopyEnvVars(Process.GetCurrentProcess(), msBuildProcess);
                writer.Flush();
                return msBuildProcess;
            }
        }

        void CopyEnvVars(Process getCurrentProcess, IProcess msBuildProcess)
        {
            foreach (string key in getCurrentProcess.StartInfo.EnvironmentVariables.Keys)
            {
                msBuildProcess.SetEnvironmentVariable(key, getCurrentProcess.StartInfo.EnvironmentVariables[key]);
            }
        }
        static bool NotTarget(string str)
        {
            str = str.TrimStart();
            return str.StartsWithNoCase("/t") == false && str.StartsWithNoCase("\"t") == false;
        }
    }
}