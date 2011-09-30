using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
        }

        protected ILookup<string, string> Properties { get; set; }

        public IEnumerable<string> Platform { get; set; }
        public IEnumerable<string> Profile { get; set; }
        public IEnumerable<string> Project { get; set; }
        public IEnumerable<string> Configuration { get; set; }
        public ICollection<IFile> ProjectFiles { get; set; }

        protected override string ExecutablePath
        {
            get { return GetMSBuildExecutablePath(); }
        }

        public bool Incremental { get; set; }

        public override IEnumerable<BuildResult> Build()
        {
            var currentDirectory = _environment.CurrentDirectory;
            ProjectFiles.Clear();
            if (Project.Count() > 0)
            {
                foreach(var proj in Project)
                {
                    var pathSpec = _environment.DescriptorFile.Parent.Path.Combine(proj).FullPath;
                    var specFiles = _fileSystem.Files(pathSpec);
                    if (specFiles.Count() == 0 || specFiles.Any(x => x.Exists == false))
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
            foreach(var project in builds)
            {
                using(var responseFile = _fileSystem.CreateTempFile())
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
                    yield return new TextBuildResult(string.Format("Building '{0}'...", project.file.Path.FullPath));


                    foreach (var m in ExecuteEngine(msbuildProcess)) yield return m;

                    yield return new TextBuildResult("Build complete.");
                }
            }
        }

        static string GetMSBuildExecutablePath()
        {
            var versionedFolders = from version in Directory.GetDirectories(Environment.ExpandEnvironmentVariables(@"%windir%\Microsoft.NET\Framework\"), "v*")
                                   orderby version descending
                                   let msbuildPath = Path.Combine(version, "msbuild.exe")
                                   where File.Exists(msbuildPath)
                                   select msbuildPath;
            return versionedFolders.FirstOrDefault();
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
                        .FirstOrDefault(x=>x.Exists);
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
            foreach(string key in getCurrentProcess.StartInfo.EnvironmentVariables.Keys)
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