// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using OpenFileSystem.IO.FileSystems.Local;
using OpenWrap.Collections;

namespace OpenWrap.Build.Tasks
{
    public class PublishPackageContent : Task
    {
        public ITaskItem[] OutputAssemblyFiles { get; set; }
        public ITaskItem[] ContentFiles { get; set; }
        public ITaskItem[] AllAssemblyReferenceFiles { get; set; }
        public ITaskItem[] OpenWrapReferenceFiles { get; set; }
        public ITaskItem[] PdbFiles { get; set; }
        public ITaskItem[] CodeDocFiles { get; set; }
        public ITaskItem[] SatelliteAssemblies { get; set; }
        public ITaskItem[] SerializationAssemblies { get; set; }
        public ITaskItem[] SourceFiles { get; set; }
        public ITaskItem[] BaseOutputPaths { get; set; }

        public bool IncludeCodeDocFiles { get; set; }
        public bool IncludePdbFiles { get; set; }
        public bool IncludeSourceFiles { get; set; }
        public bool AllowBinDuplicates { get; set; }
        public string ProjectPath { get; set; } 

        public string ExportName { get; set; }

        public override bool Execute()
        {
            WriteLow("IncludeDocumentation: " + IncludeCodeDocFiles);
            WriteLow("IncludePdbs: " + IncludePdbFiles);
            WriteLow("IncludeSourceFiles: " + IncludeSourceFiles);
            WriteLow("ExportName: " + ExportName);
            WriteLow("AllowBinDuplicates: " + AllowBinDuplicates);

            WriteLow("BaseOutputPaths: " + BaseOutputPaths);

            WriteFiles("OutputAssemblyFiles", OutputAssemblyFiles);
            WriteFiles("ContentFiles", ContentFiles);
            WriteFiles("AllAssemblyReferenceFiles", AllAssemblyReferenceFiles);
            WriteFiles("OpenWrapReferenceFiles", OpenWrapReferenceFiles);
            WriteFiles("PdbFiles", PdbFiles);
            WriteFiles("CodeDocFiles", CodeDocFiles);
            WriteFiles("SourceFiles", SourceFiles);
            WriteFiles("SatelliteAssemblies", SatelliteAssemblies);
            WriteFiles("SerializationAssemblies", SerializationAssemblies);

            var emitter = new MSBuildInstructionEmitter(LocalFileSystem.Instance)
            {
                    AllAssemblyReferenceFiles = Files(AllAssemblyReferenceFiles),
                    ContentFiles = Files(ContentFiles),
                    OpenWrapReferenceFiles = Files(OpenWrapReferenceFiles),
                    PdbFiles = Files(PdbFiles),
                    CodeDocFiles = Files(CodeDocFiles),
                    SourceFiles = Files(SourceFiles),
                    SatelliteAssemblies = Files(SatelliteAssemblies),
                    SerializationAssemblies = Files(SerializationAssemblies),
                    OutputAssemblyFiles = Files(OutputAssemblyFiles),
                    BaseOutputPaths = Files(BaseOutputPaths),
                    IncludePdbFiles = IncludePdbFiles,
                    IncludeCodeDocFiles = IncludeCodeDocFiles,
                    IncludeSourceFiles = IncludeSourceFiles,
                    ExportName = ExportName,
                    ProjectPath = ProjectPath
            };
            foreach (var kv in emitter.GenerateInstructions())
                BuildEngine.LogMessageEvent(new BuildMessageEventArgs(
                                                         string.Format("[built({0}, '{1}', {2})]", kv.Key, kv.Value, AllowBinDuplicates.ToString().ToLowerInvariant()),
                                                         null,
                                                         "OpenWrap",
                                                         MessageImportance.Normal));
            return true;
        }

        void WriteFiles(string categoryName, IEnumerable<ITaskItem> taskItems)
        {
            if (taskItems == null)
                return;

            foreach (var file in taskItems)
                WriteLow(string.Format("{0}: {1}", categoryName, file.ItemSpec));
        }

        void WriteLow(string message)
        {
            BuildEngine.LogMessageEvent(new BuildMessageEventArgs(
                                                message,
                                                null,
                                                "OpenWrap",
                                                MessageImportance.Low));
        }

        static List<string> Files(IEnumerable<ITaskItem> specs)
        {
            return specs == null
                ? new List<string>(0)
                : specs.NotNull()
                       .Select(x=>System.IO.Path.GetFullPath(x.ItemSpec))
                       .ToList();
        }
    }
}
// ReSharper restore UnusedAutoPropertyAccessor.Global
// ReSharper restore MemberCanBePrivate.Global