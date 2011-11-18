using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenFileSystem.IO;
using OpenWrap.Collections;

namespace OpenWrap.Build.Tasks
{
    public class MSBuildInstructionEmitter
    {
        readonly IFileSystem _fileSystem;
        public bool IncludePdbFiles { get; set; }
        public bool IncludeCodeDocFiles { get; set; }
        public string ExportName { get; set; }

        public MSBuildInstructionEmitter(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            IncludePdbFiles = true;
            IncludeCodeDocFiles = true;
            IncludeSourceFiles = false;
            IncludeBinFiles = true;
            BuildOutputFiles = Lookup<string, string>.Empty;
            ContentFiles = Lookup<string, string>.Empty;
            SourceFiles = Lookup<string, string>.Empty;
        }

        public bool IncludeSourceFiles { get; set; }

        public ILookup<string, string> ContentFiles { get; set; }
        public ILookup<string, string> SourceFiles { get; set; }
        public ILookup<string, string> BuildOutputFiles { get; set; }

        public string RootPath { get; set; }

        public bool IncludeBinFiles { get; set; }

        public bool IncludeContentFiles { get; set; }


        public IEnumerable<KeyValuePair<string, string>> GenerateInstructions()
        {
            if (string.IsNullOrEmpty(RootPath))
                return Enumerable.Empty<KeyValuePair<string, string>>();
            if (string.IsNullOrEmpty(ExportName)) throw new InvalidOperationException("ExportName is not defined.");
            VerifyAbsolutePaths();

            return GenerateInstructionsCore();
        }

        IEnumerable<KeyValuePair<string, string>> GenerateInstructionsCore()
        {
            if (IncludeBinFiles)
                foreach (var source in from export in BuildOutputFiles
                                       from filePath in export
                                       let file = _fileSystem.GetFile(filePath)
                                       let packagePath = GetPackagePath(ExportName, export.Key, file)
                                       where file.Exists && FileMatchesDesiredFlavour(file)
                                       select new { packagePath, file })

                    yield return GetKey(source.packagePath, source.file.Path);


            if (IncludeContentFiles)
                foreach (var source in from export in ContentFiles
                                       from filePath in export
                                       let file = _fileSystem.GetFile(filePath)
                                       let packagePath = GetPackagePath("content", export.Key, file)
                                       where file.Exists
                                       select new { packagePath, file })
                    yield return GetKey(source.packagePath, source.file.Path);
            var rootPath = new OpenFileSystem.IO.Path(RootPath);
            var sourcePath = new OpenFileSystem.IO.Path("source");

            if (IncludeSourceFiles)
                foreach (var source in from exportPath in SourceFiles
                                       from filePath in exportPath
                                       let file = _fileSystem.GetFile(filePath)
                                       where file.Exists && IsBasePath(rootPath, file.Path)
                                       select new { packagePath = sourcePath.Combine(file.Parent.Path.MakeRelative(rootPath)), file })
                    yield return GetKey(source.packagePath, source.file.Path);

        }

        string GetPackagePath(string baseExport, string exportKey, IFile file)
        {
            if (exportKey == ".") return baseExport;
            return new OpenFileSystem.IO.Path(baseExport).Combine(exportKey);
        }

        bool FileMatchesDesiredFlavour(IFile file)
        {
            if (!IncludePdbFiles && file.Extension.EqualsNoCase(".pdb")) return false;
            if (!IncludeCodeDocFiles && file.Extension.EqualsNoCase(".xml")) return false;
            if (!IncludeBinFiles && ContainsCIL(file)) return false;

            return true;
        }

        void VerifyAbsolutePaths(ILookup<string, string> paths, string message)
        {
            var invalid = paths.Select(_ => _).SelectMany(_ => _)
                .Where(_ => new OpenFileSystem.IO.Path(_).IsRooted == false)
                .JoinString(", ");
            if (invalid.Length > 0)
                throw new InvalidOperationException(string.Format("The following paths in  {0} are not rooted: {1}", message, invalid));
        }

        void VerifyAbsolutePaths()
        {
            VerifyAbsolutePaths(SourceFiles, "SourceFiles");
            VerifyAbsolutePaths(ContentFiles, "ContentFiles");
            VerifyAbsolutePaths(BuildOutputFiles, "BuildOutputFiles");
        }

        KeyValuePair<string, string> GetKey(string exportPath, OpenFileSystem.IO.Path absolutePath)
        {
            return Key(exportPath, absolutePath);
        }

        bool IsBasePath(OpenFileSystem.IO.Path path, OpenFileSystem.IO.Path absolutePath)
        {
            var basePathSegments = path.Segments.ToList();
            var absolutePathSegments = absolutePath.Segments.ToList();
            if (absolutePathSegments.Count <= basePathSegments.Count) return false;

            for (int i = 0; i < path.Segments.Count(); i++)
            {
                if (basePathSegments[i] != absolutePathSegments[i]) return false;
            }
            return true;
        }


        bool ContainsCIL(IFile file)
        {
            return file.Extension.EqualsNoCase(".dll")
                   || file.Extension.EqualsNoCase(".exe")
                   || file.Extension.EqualsNoCase(".netmodule");
        }
        static KeyValuePair<string, string> Key(string name, string value)
        {
            return new KeyValuePair<string, string>(name, value);
        }
    }
}