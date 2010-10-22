using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems;

namespace OpenWrap.Build.BuildEngines
{
    public class ConventionMSBuildEngine : IPackageBuilder
    {
        readonly IFileSystem _fileSystem;
        readonly IEnvironment _environment;
        readonly BuiltInstructionParser _parser = new BuiltInstructionParser();
        public IEnumerable<string> Profile { get; set; }
        public IEnumerable<string> Platform { get; set; }
        public IEnumerable<string> Project { get; set; }
        public ConventionMSBuildEngine(IFileSystem fileSystem, IEnvironment environment)
        {
            _fileSystem = fileSystem;
            _environment = environment;
            Profile = new List<string> { environment.ExecutionEnvironment.Profile };
            Platform = new List<string> { environment.ExecutionEnvironment.Platform };
            Project = new List<string>();
        }

        public IEnumerable<BuildResult> Build()
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
            foreach (var project in builds)
            {
                var msbuildProcess = CreateMSBuildProcess(project.file, project.platform, project.profile);
                yield return new TextBuildResult(string.Format("Using MSBuild from path '{0}'.", msbuildProcess.StartInfo.FileName));
                msbuildProcess.Start();
                var reader = msbuildProcess.StandardOutput;

                yield return new TextBuildResult(string.Format("Building '{0}'...", project.file.Path.FullPath));
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var parsed = _parser.Parse(line);
                    if (parsed.Count() > 0)
                        foreach (var m in parsed)
                            yield return m;
                    else
                        yield return new TextBuildResult(line);
                }
                msbuildProcess.WaitForExit();
                if (msbuildProcess.ExitCode != 0)
                    yield return new ErrorBuildResult();

                yield return new TextBuildResult("Build complete.");
            }
        }

        static Process CreateMSBuildProcess(IFile project, string platform, string profile)
        {
            var msbuildParams = string.Format(" /p:OpenWrap-EmitOutputInstructions=true /p:OpenWrap-TargetPlatform={0} /p:OpenWrap-TargetProfile={1}", platform, profile);

            return new Process
            {
                    StartInfo = new ProcessStartInfo
                    {
                            RedirectStandardOutput = true,
                            FileName = GetMSBuildExecutableName(),
                            Arguments = "\"" + project.Path.FullPath + "\"" + msbuildParams,
                            UseShellExecute = false
                    }
            };
        }

        static string GetMSBuildExecutableName()
        {
            var versionedFolders = from version in Directory.GetDirectories(Environment.ExpandEnvironmentVariables(@"%windir%\Microsoft.NET\Framework\"), "v*")
                                   orderby version descending
                                   let msbuildPath = Path.Combine(version, "msbuild.exe")
                                   where File.Exists(msbuildPath)
                                   select msbuildPath;
            return versionedFolders.FirstOrDefault();
        }
    }
}