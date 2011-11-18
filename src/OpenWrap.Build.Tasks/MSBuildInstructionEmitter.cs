using System;
using System.Collections.Generic;
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
            ContentFiles = new List<string>();
            AllAssemblyReferenceFiles = new List<string>();
            OpenWrapReferenceFiles = new List<string>();
            PdbFiles = new List<string>();
            CodeDocFiles = new List<string>();
            SatelliteAssemblies = new List<string>();
            SerializationAssemblies = new List<string>();
            OutputAssemblyFiles = new List<string>();
            IncludePdbFiles = true;
            IncludeCodeDocFiles = true;
            IncludeSourceFiles = false;
            SourceFiles = new List<string>();
        }

        public bool IncludeSourceFiles { get; set; }

        public ICollection<string> ContentFiles { get; set; }
        public ICollection<string> AllAssemblyReferenceFiles { get; set; }
        public ICollection<string> OpenWrapReferenceFiles { get; set; }
        public ICollection<string> PdbFiles { get; set; }
        public ICollection<string> CodeDocFiles { get; set; }
        public ICollection<string> SatelliteAssemblies { get; set; }
        public ICollection<string> SerializationAssemblies { get; set; }

        public ICollection<string> OutputAssemblyFiles { get; set; }

        public ICollection<string> SourceFiles { get; set; }

        public string ProjectPath { get; set; }

        public ICollection<string> BaseOutputPaths { get; set; }

        public IEnumerable<KeyValuePair<string, string>> GenerateInstructions()
        {
            if (BaseOutputPaths == null || BaseOutputPaths.Count == 0) throw new InvalidOperationException("BaseOutputPaths is not defined or empty.");
            if (string.IsNullOrEmpty(ExportName)) throw new InvalidOperationException("ExportName is not defined.");

            return GenerateInstructionsCore();
        }

        IEnumerable<KeyValuePair<string, string>> GenerateInstructionsCore()
        {
            var binaryOutputPath = BaseOutputPaths.Select(_ => _fileSystem.GetDirectory(_))
                                                 .Where(_ => _.Exists)
                                                 .Select(_=>_.Path)
                                                 .ToArray();
            var projectPath = _fileSystem.GetDirectory(ProjectPath);
            var includedAssemblies = AllAssemblyReferenceFiles
                .Where(x => !IsOpenWrapReferenceOrAssociatedFile(x))
                .Select(_fileSystem.GetFile)
                .Where(_=>_.Exists)
                .Concat(OutputAssemblyFiles.Select(_fileSystem.GetFile))
                .Where(IsNetAssembly)
                .ToList();


            // Assemblies in bin- (target assembly + project refs)

            foreach (var file in includedAssemblies)
                yield return GetKey(ExportName, file.Path, binaryOutputPath);

            var associatedFiles = Enumerable.Empty<string>();

            if (IncludeCodeDocFiles) associatedFiles = associatedFiles.Concat(CodeDocFiles);
            if (IncludePdbFiles) associatedFiles = associatedFiles.Concat(PdbFiles);

            foreach (var associated in associatedFiles)
            {
                var associatedFile = _fileSystem.GetFile(associated);
                if (IsFileMatchingIncludedAssembly(includedAssemblies, associatedFile, _ => _))
                    yield return GetKey(ExportName, associatedFile.Path, binaryOutputPath);
            }


            foreach (var filePath in ContentFiles)
                yield return GetKey(ExportName, new Path(filePath), projectPath.Path);

            if (IncludeSourceFiles)
                foreach (var source in SourceFiles)
                    yield return GetKey("src", new Path(source), projectPath.Path);

            foreach (var key in TryOutput(SatelliteAssemblies,
                                          includedAssemblies,
                                          ".resources",
                                          binaryOutputPath))
                yield return key;

            foreach (var key in TryOutput(SerializationAssemblies,
                                          includedAssemblies,
                                          ".XmlSerializers",
                                          binaryOutputPath))
                yield return key;
            foreach (var file in SatelliteAssemblies.Select(_fileSystem.GetFile))
            {
                if (IsFileMatchingIncludedAssembly(includedAssemblies,
                    file,
                    x => TryRemoveSuffix(x, ".resources")))
                    yield return GetKey(ExportName, file.Path, binaryOutputPath);
            }
        }
        IEnumerable<KeyValuePair<string,string>> TryOutput(IEnumerable<string> filePaths, IEnumerable<IFile> includedAssemblies, string suffix, Path[] basePaths)
        {
            foreach (var file in filePaths.Select(_fileSystem.GetFile))
            {
                if (IsFileMatchingIncludedAssembly(includedAssemblies,
                    file,
                    x => TryRemoveSuffix(x, suffix)))
                    yield return GetKey(ExportName, file.Path, basePaths);
            }
        } 
        KeyValuePair<string, string> GetKey(string exportPath, Path absolutePath, params Path[] basePath)
        {
            if (basePath.Length == 0) return Key(exportPath, absolutePath);
            var actualBasePath = basePath.FirstOrDefault(_ => IsBasePath(_, absolutePath));
            if (actualBasePath == null) return Key(exportPath, absolutePath);

            var relativeContentPath = absolutePath.MakeRelative(actualBasePath);
            var packageRelativePath = new Path(exportPath).Combine(relativeContentPath).DirectoryName;
            return Key(packageRelativePath, absolutePath);
        }

        bool IsBasePath(Path path, Path absolutePath)
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

        bool IsOpenWrapReferenceOrAssociatedFile(string filePath)
        {
            if (OpenWrapReferenceFiles.Contains(filePath))
            {
                return true;
            }

            var baseFileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
            var suffices = new[] { ".Contracts", ".XmlSerializers", ".resources" };

            var referenceFileName = suffices.Select(suffix => TryRemoveSuffix(baseFileName, suffix)).NotNull().FirstOrDefault();
            
            return OpenWrapReferenceFiles.ContainsNoCase(referenceFileName);
        }



        static IFile GetRelatedFile(IEnumerable<IFile> includedAssemblies, IFile file, Func<string, string> converter)
        {
            var converted = converter(file.NameWithoutExtension);

            return converted == null
                ? null
                : includedAssemblies.SingleOrDefault(x => x.NameWithoutExtension.EqualsNoCase(converted));

        }


        bool IsNetAssembly(IFile file)
        {
            return file.Extension.EqualsNoCase(".dll")
                   || file.Extension.EqualsNoCase(".exe")
                   || file.Extension.EqualsNoCase(".netmodule");
        }

        static string TryRemoveSuffix(string arg, string suffix)
        {
            return arg.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
                           ? arg.Substring(0, arg.Length - suffix.Length)
                           : null;
        }

        static bool IsFileMatchingIncludedAssembly(IEnumerable<IFile> includedAssemblies, IFile file, Func<string, string> converter)
        {
            var converted = converter(file.NameWithoutExtension);

            return converted != null && includedAssemblies.Any(x => x.NameWithoutExtension.EqualsNoCase(converted));
        }

        static KeyValuePair<string, string> Key(string name, string value)
        {
            return new KeyValuePair<string, string>(name, value);
        }
    }
}