using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems;
using OpenWrap.Commands;

namespace OpenWrap.Build.BuildEngines
{
<<<<<<< HEAD
    public class BuiltInstructionParser
    {
        static Regex _buildInstructionRegex = new Regex(@"\[built\((<?export>.+)(\,(?<fileSpec>.*))\)\]");
        
        public IEnumerable<FileBuildResult> Parse(string line)
        {
            var instructionMatch = _buildInstructionRegex.Match(line);
            if (instructionMatch.Success)
            {
                var fileSpec = instructionMatch.Groups["fileSpec"];
                var exportName = instructionMatch.Groups["export"];
                if (fileSpec.Success && exportName.Success)
                {
                    return (
                                from x in fileSpec.Value.Split(new[]{";"},StringSplitOptions.RemoveEmptyEntries)
                                select new FileBuildResult(exportName.Value.Trim(), new LocalPath(x))
                           )
                           .ToList();
                    
                }
            }
            return Enumerable.Empty<FileBuildResult>();
        }
    }
=======
>>>>>>> f73f0dce227b51986ff52ba6fb3db3b2a48c748f
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
<<<<<<< HEAD
                var msbuildProcess = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                            RedirectStandardOutput = true,
                            FileName = GetMSBuildFileName(),
                            Arguments = project.Path.FullPath,
                            UseShellExecute = false
                    }                    
                };
                msbuildProcess.Start();
                var reader = msbuildProcess.StandardOutput;
                yield return new TextBuildResult(string.Format("Building '{0}'...", project.Path.FullPath));

                var content = reader.ReadToEnd().Split(new[]{'\r','\n'}, StringSplitOptions.RemoveEmptyEntries);
                foreach(var line in content)
                {
                    //yield return new TextBuildResult(line);
                    foreach(var m in _parser.Parse(line)) yield return m;
                }
=======
                var msbuildProcess = CreateMSBuildProcess(project);
                msbuildProcess.Start();
                var reader = msbuildProcess.StandardOutput;

                yield return new TextBuildResult(string.Format("Building '{0}'...", project.Path.FullPath));

                var content = reader.ReadToEnd().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var m in content.SelectMany(line => _parser.Parse(line)))
                    yield return m;

>>>>>>> f73f0dce227b51986ff52ba6fb3db3b2a48c748f
                yield return new TextBuildResult("Built...");
            }
        }

        Process CreateMSBuildProcess(IFile project)
        {
<<<<<<< HEAD
            return @"C:\windows\Microsoft.NET\Framework\v3.5\msbuild.exe";
        }
    }
    public class BuildResult
    {
    }
    public class TextBuildResult : BuildResult
    {
        public TextBuildResult(string text)
        {
            Text = text;
=======
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
>>>>>>> f73f0dce227b51986ff52ba6fb3db3b2a48c748f
        }

        string GetMSBuildExecutableName()
        {
            return Environment.ExpandEnvironmentVariables(@"%windir%\Microsoft.NET\Framework\v3.5\msbuild.exe");
        }
    }
}