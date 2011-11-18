using System;
using System.IO;
using System.Linq;
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

        public msbuild_emitter()
        {
            _fileSystem = new InMemoryFileSystem();
            _emitter = new MSBuildInstructionEmitter(_fileSystem);
            _emitter.ProjectPath = Path.GetTempPath();
            _emitter.BaseOutputPaths = new[]{System.IO.Path.Combine(_emitter.ProjectPath, "bin")};
        }
        protected void given_assembly_reference(string path)
        {
            _emitter.AllAssemblyReferenceFiles.Add(path);
        }
        protected void given_openwrap_reference(string path)
        {
            _emitter.OpenWrapReferenceFiles.Add(path);
        }
        protected void given_source_file(string path)
        {
            _emitter.SourceFiles.Add(path);
        }
        protected void given_content_file(string path)
        {
            _emitter.ContentFiles.Add(path);
        }
        protected void given_documentation_file(string path)
        {
            _emitter.CodeDocFiles.Add(path);
        }
        protected void given_satellite(string path)
        {
            _emitter.SatelliteAssemblies.Add(path);

        }
        protected void given_serialization(string path)
        {
            _emitter.SerializationAssemblies.Add(path);

        }
        protected void given_pdb(string path)
        {
            _emitter.PdbFiles.Add(path);
        }
        protected void when_generating_instructions()
        {
            Instructions = _emitter.GenerateInstructions().ToLookup(x => x.Key, x => x.Value);

        }
        protected void should_have_file(string folder, string filePath)
        {
            Instructions[folder].ShouldContain(RootedPath(filePath));
        }
        protected void should_not_have_file(string folder, string filePath)
        {
            Instructions[folder].ShouldNotContain(RootedPath(filePath));
        }

        protected void given_export_name(string exportName)
        {
            _emitter.ExportName = exportName;
        }

        protected string RootedPath(string fileName)
        {
            return Path.Combine(_emitter.ProjectPath, fileName);
        }

        protected void given_output_assembly(string assemblyPath)
        {
            _emitter.OutputAssemblyFiles.Add(RootedPath(assemblyPath));
        }

        protected void given_project_path(string basePath)
        {
            _emitter.ProjectPath = basePath;
        }
        protected void given_includes(bool? pdbs = null, bool? source = null, bool? codeDoc = null)
        {
            if (pdbs != null) _emitter.IncludePdbFiles = pdbs.Value;
            if (source != null) _emitter.IncludeSourceFiles = source.Value;
            if (codeDoc != null) _emitter.IncludeCodeDocFiles = codeDoc.Value;
        }
    }
}