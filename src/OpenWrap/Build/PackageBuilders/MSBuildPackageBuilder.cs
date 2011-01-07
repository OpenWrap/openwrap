using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using OpenFileSystem.IO;
using OpenWrap.IO;
using OpenWrap.Runtime;
using Path = System.IO.Path;

namespace OpenWrap.Build.PackageBuilders
{
    public class MSBuildPackageBuilder : AbstractProcessPackageBuilder
    {
        public MSBuildPackageBuilder(IFileSystem fileSystem, IEnvironment environment, IFileBuildResultParser parser)
                : base(fileSystem, environment, parser)

        {
            Profile = new List<string> { environment.ExecutionEnvironment.Profile };
            Platform = new List<string> { environment.ExecutionEnvironment.Platform };
            Project = new List<string>();
        }

        public IEnumerable<string> Platform { get; set; }
        public IEnumerable<string> Profile { get; set; }
        public IEnumerable<string> Project { get; set; }

        protected override string ExecutablePath
        {
            get { return GetMSBuildExecutablePath(); }
        }

        public override IEnumerable<BuildResult> Build()
        {
            var currentDirectory = _environment.CurrentDirectory;

            IEnumerable<IFile> projectFiles;
            if (Project.Count() > 0)
            {
                projectFiles = Project.Select(x => _fileSystem.GetFile(_environment.DescriptorFile.Parent.Path.Combine(x).FullPath)).Where(x => x.Exists);
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
                projectFiles = sourceDirectory.Files("*.*proj", SearchScope.SubFolders);
            }

            var builds = from file in projectFiles
                         from platform in Platform.DefaultIfEmpty(null)
                         from profile in Profile.DefaultIfEmpty(null)
                         select new { file, platform, profile };

            yield return new TextBuildResult(string.Format("Using MSBuild from path '{0}'.", ExecutablePath));

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

        Process CreateMSBuildProcess(IFile responseFile, IFile project, string platform, string profile, string msbuildTargets)
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
                    var msbuildVersioning = FrameworkVersioning.OpenWrapToMSBuild(profile);
                    writer.WriteLine("/p:OpenWrap-TargetProfile=" + profile);
                    writer.WriteLine("/p:TargetFrameworkVersion=" + msbuildVersioning.Version);
                    writer.WriteLine("/p:TargetFrameworkIdentifier=" + msbuildVersioning.Identifier);
                    writer.WriteLine("/p:TargetFrameworkProfile=" + msbuildVersioning.Profile);
                }
                writer.WriteLine("/p:OpenWrap-EmitOutputInstructions=true");
                writer.WriteLine("/p:OpenWrap-CurrentProjectFile=" + project.Path.FullPath);
                if (msbuildTargets != null)
                    writer.WriteLine("/t:" + msbuildTargets);

                var args = string.Format(@"@""{1}"" ""{0}""", project.Path.FullPath, responseFile.Path.FullPath);
                var msBuildProcess = CreateProcess(args);
                CopyEnvVars(Process.GetCurrentProcess(), msBuildProcess);
                writer.Flush();
                return msBuildProcess;
            }
        }

        void CopyEnvVars(Process getCurrentProcess, Process msBuildProcess)
        {
            foreach(string key in getCurrentProcess.StartInfo.EnvironmentVariables.Keys)
            {
                msBuildProcess.StartInfo.EnvironmentVariables[key] = getCurrentProcess.StartInfo.EnvironmentVariables[key];
            }
        }
        static bool NotTarget(string str)
        {
            str = str.TrimStart();
            return str.StartsWithNoCase("/t") == false && str.StartsWithNoCase("\"t") == false;
        }
    }
}