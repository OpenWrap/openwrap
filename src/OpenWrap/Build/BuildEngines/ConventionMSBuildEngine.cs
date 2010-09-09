using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems;

namespace OpenWrap.Build.BuildEngines
{
    public class ConventionMSBuildEngine
    {
        readonly IEnvironment _environment;
        readonly BuiltInstructionParser _parser = new BuiltInstructionParser();

        public ConventionMSBuildEngine(IEnvironment environment)
        {
            _environment = environment;
        }

        public IEnumerable<BuildResult> Build()
        {
            var currentDirectory = _environment.CurrentDirectory;
            var sourceDirectory = currentDirectory.GetDirectory("src");
            if (!sourceDirectory.Exists)
            {
                yield return
                        new TextBuildResult(string.Format("Could not locate a /src folder in current directory '{0}'. Make sure you use the default layout for project code.",
                                                          _environment.CurrentDirectory.Path.FullPath));
                yield break;
            }
            foreach (var project in sourceDirectory.Files("*.*proj", SearchScope.SubFolders))
            {
                var msbuildProcess = CreateMSBuildProcess(project);
                msbuildProcess.Start();
                var reader = msbuildProcess.StandardOutput;

                yield return new TextBuildResult(string.Format("Building '{0}'...", project.Path.FullPath));

                var content = reader.ReadToEnd().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var m in content.SelectMany(line => _parser.Parse(line)))
                    yield return m;

                yield return new TextBuildResult("Built...");
            }
        }

        Process CreateMSBuildProcess(IFile project)
        {
            return new Process
            {
                    StartInfo = new ProcessStartInfo
                    {
                            RedirectStandardOutput = true,
                            FileName = GetMSBuildExecutableName(),
                            Arguments = project.Path.FullPath + " /p:OpenWrap-EmitOutputInstructions=true",
                            UseShellExecute = false
                    }
            };
        }

        string GetMSBuildExecutableName()
        {
            return Environment.ExpandEnvironmentVariables(@"%windir%\Microsoft.NET\Framework\v3.5\msbuild.exe");
        }
    }
}