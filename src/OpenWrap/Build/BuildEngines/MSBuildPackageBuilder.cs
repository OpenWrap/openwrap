using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using OpenFileSystem.IO;

namespace OpenWrap.Build.BuildEngines
{
    public class MSBuildPackageBuilder : AbstractProcessPackageBuilder
    {
        public IEnumerable<string> Profile { get; set; }
        public IEnumerable<string> Platform { get; set; }
        public IEnumerable<string> Project { get; set; }
        protected override string ExecutablePath
        {
            get { return GetMSBuildExecutablePath(); }
        }
        public MSBuildPackageBuilder(IFileSystem fileSystem, IEnvironment environment)
            : base(fileSystem,environment)

        {
            Profile = new List<string> { environment.ExecutionEnvironment.Profile };
            Platform = new List<string> { environment.ExecutionEnvironment.Platform };
            Project = new List<string>();
        }

        public override IEnumerable<BuildResult> Build()
        {
            var currentDirectory = _environment.CurrentDirectory;
            var sourceDirectory = currentDirectory.GetDirectory("src");
            if (!sourceDirectory.Exists)
            {
                yield return
                        new ErrorBuildResult(string.Format("Could not locate a /src folder in current directory '{0}'. Make sure you use the default layout for project code.",
                                                          _environment.CurrentDirectory.Path.FullPath));
                yield break;
            }
            
            var projectFiles = (Project.Count() > 0)
                ? Project.Select(x=> _fileSystem.GetFile(_environment.DescriptorFile.Parent.Path.Combine(x).FullPath)).Where(x=>x.Exists)
                : sourceDirectory.Files("*.*proj", SearchScope.SubFolders);

            var builds = from file in projectFiles
                         from platform in Platform
                         from profile in Profile
                         select new { file, platform, profile };

            yield return new TextBuildResult(string.Format("Using MSBuild from path '{0}'.", ExecutablePath));

            foreach(var m in builds
                .SelectMany(project=>ExecuteEngine(CreateMSBuildProcess(project.file, project.platform, project.profile, "Clean"))))
                    yield return m;
            
            foreach (var project in builds)
            {
                var msbuildProcess = CreateMSBuildProcess(project.file, project.platform, project.profile, "Build");
                yield return new TextBuildResult(string.Format("Building '{0}'...", project.file.Path.FullPath));


                foreach (var m in ExecuteEngine(msbuildProcess)) yield return m;

                yield return new TextBuildResult("Build complete.");
            }
        }

        Process CreateMSBuildProcess(IFile project, string platform, string profile, string msbuildTargets)
        {
            var commandLineArgs = Environment.GetCommandLineArgs();
            var logger = commandLineArgs.FirstOrDefault(x => x.StartsWith("/logger", StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
            var additionalProperties = (from argument in commandLineArgs
                                        where argument.StartsWithNoCase("/p:") ||
                                              argument.StartsWithNoCase("/property:") ||
                                              argument.StartsWithNoCase("\"/property:") ||
                                              argument.StartsWithNoCase("\"/p:")
                                        select argument).Join(" ");

            var msbuildParams = string.Format(" /p:OpenWrap-EmitOutputInstructions=true /p:OpenWrap-TargetPlatform={0} /p:OpenWrap-TargetProfile={1} /p:t={2} {3} {4}", platform, profile, msbuildTargets, logger, additionalProperties);

            var args = "\"" + project.Path.FullPath + "\"" + msbuildParams;
            var msBuildProcess = CreateProcess(args);
            msBuildProcess.StartInfo.EnvironmentVariables["TEAMCITY_BUILD_PROPERTIES_FILE"]
                    = Process.GetCurrentProcess().StartInfo.EnvironmentVariables["TEAMCITY_BUILD_PROPERTIES_FILE"];
            return msBuildProcess;
        }

        static string GetMSBuildExecutablePath()
        {
            var versionedFolders = from version in Directory.GetDirectories(Environment.ExpandEnvironmentVariables(@"%windir%\Microsoft.NET\Framework\"), "v*")
                                   orderby version descending
                                   let msbuildPath = System.IO.Path.Combine(version, "msbuild.exe")
                                   where File.Exists(msbuildPath)
                                   select msbuildPath;
            return versionedFolders.FirstOrDefault();
        }
    }
}