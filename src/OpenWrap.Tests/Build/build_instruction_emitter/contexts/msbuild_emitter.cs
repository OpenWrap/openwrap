using System;
using System.Collections.Generic;
using System.Linq;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.InMemory;
using OpenWrap.Build.Tasks;
using OpenWrap.Testing;

namespace Tests.Build.build_instruction_emitter.contexts
{
    public class msbuild_emitter
    {
        MSBuildInstructionEmitter _emitter;
        protected ILookup<string, string> Instructions;
        InMemoryFileSystem _fileSystem;
        OpenFileSystem.IO.Path BinPath;
        protected ITemporaryDirectory BinDirectory;
        protected ITemporaryDirectory TempDirectory;
        ITemporaryDirectory GacDirectory;
        IDirectory RootDirectory;
        IFile ProjectFile;
        IDirectory ProjectDirectory;
        Dictionary<string, List<string>> SourceFiles = new Dictionary<string, List<string>>();
        Dictionary<string, List<string>> OutputFiles = new Dictionary<string, List<string>>();
        Dictionary<string, List<string>> ContentFiles = new Dictionary<string, List<string>>();

        public msbuild_emitter()
        {
            _fileSystem = new InMemoryFileSystem();
            _emitter = new MSBuildInstructionEmitter(_fileSystem);
            RootDirectory = _fileSystem.CreateTempDirectory();
            _emitter.RootPath = RootDirectory.Path;
            ProjectDirectory = RootDirectory.GetDirectory("src").GetDirectory("Project");
            ProjectFile = ProjectDirectory.GetFile("Project.csproj");

            BinDirectory 
                = _fileSystem.CreateTempDirectory();
            GacDirectory = _fileSystem.CreateTempDirectory();
            TempDirectory = _fileSystem.CreateTempDirectory();
            BinPath = new OpenFileSystem.IO.Path(System.IO.Path.Combine(_emitter.RootPath, "bin"));
        }
        List<string> GetFiles(string path, Dictionary<string,List<string>> source)
        {
            List<string> result;
            if (!source.TryGetValue(path, out result))
            {
                source[path] = result = new List<string>();

            }
            return result;
        }

        protected void given_output(string fileName)
        {
            given_output(".", fileName);
        }

        protected void given_output(string relativePath, string fileName)
        {
            GetFiles(relativePath, OutputFiles).Add(TempDirectory.Path.Combine(fileName));

        }

        protected void given_source_file(string fileName)
        {
            given_source_file(".", fileName);
        }

        protected void given_source_file(string relativePath, string fileName)
        {
            GetFiles(relativePath, SourceFiles).Add(TempDirectory.Path.Combine(fileName));
        }

        protected void given_content_file(string fileName)
        {
            given_content_file(".", fileName);
        }

        protected void given_content_file(string relativePath, string fileName)
        {
            GetFiles(relativePath, ContentFiles).Add(TempDirectory.Path.Combine(fileName));
        }

        protected void when_generating_instructions()
        {
            _emitter.BuildOutputFiles = ToLookup(OutputFiles);
            _emitter.SourceFiles = ToLookup(SourceFiles);
            _emitter.ContentFiles = ToLookup(ContentFiles);

            Instructions = _emitter.GenerateInstructions().ToLookup(x => x.Key, x => x.Value);

        }

        ILookup<string, string> ToLookup(Dictionary<string, List<string>> outputFiles)
        {
            return (from entry in outputFiles   
                    from filepath in entry.Value
                    let file = _fileSystem.GetFile(filepath).MustExist()
                    select new { path = entry.Key, filepath }).ToLookup(_=>_.path, _=>_.filepath);
        }

        protected void should_have_file(string folder, string filePath)
        {
            Instructions[folder].Select(System.IO.Path.GetFileName).ShouldContain(System.IO.Path.GetFileName(filePath));
        }
        protected void should_not_have_file(string folder, string filePath)
        {
            Instructions[folder].Select(System.IO.Path.GetFileName).ShouldNotContain(System.IO.Path.GetFileName(filePath));

        }

        protected void given_export_name(string exportName)
        {
            _emitter.ExportName = exportName;
        }

        protected void given_root_path(string basePath)
        {
            RootDirectory = _fileSystem.GetDirectory(basePath);
            _emitter.RootPath = RootDirectory.Path;
        }
        protected void given_includes(bool? bin = null, bool? pdbs = null, bool? source = null, bool? codeDoc = null, bool? content = null)
        {
            if (pdbs != null) _emitter.IncludePdbFiles = pdbs.Value;
            if (source != null) _emitter.IncludeSourceFiles = source.Value;
            if (codeDoc != null) _emitter.IncludeCodeDocFiles = codeDoc.Value;
            if (content != null) _emitter.IncludeContentFiles = content.Value;
            if (bin != null) _emitter.IncludeBinFiles = bin.Value;
        }

    }
}