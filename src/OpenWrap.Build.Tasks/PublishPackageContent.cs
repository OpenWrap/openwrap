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

        public bool IncludeCodeDocFiles { get; set; }
        public bool IncludePdbFiles { get; set; }
        public bool IncludeSourceFiles { get; set; }
        public string BasePath { get; set; }
        public bool AllowBinDuplicates { get; set; }

        public string ExportName { get; set; }

        public override bool Execute()
        {
            WriteLow("IncludeDocumentation: " + IncludeCodeDocFiles);
            WriteLow("IncludePdbs: " + IncludePdbFiles);
            WriteLow("BasePath: " + BasePath);
            WriteLow("ExportName: " + BasePath);
            WriteLow("AllowBinDuplicates: " + AllowBinDuplicates);

            WriteFiles("OutputAssemblyFiles", OutputAssemblyFiles);
            WriteFiles("ContentFiles", ContentFiles);
            WriteFiles("AllAssemblyReferenceFiles", AllAssemblyReferenceFiles);
            WriteFiles("OpenWrapReferenceFiles", OpenWrapReferenceFiles);
            WriteFiles("PdbFiles", PdbFiles);
            WriteFiles("CodeDocFiles", CodeDocFiles);
            WriteFiles("SatelliteAssemblies", SatelliteAssemblies);
            WriteFiles("SerializationAssemblies", SerializationAssemblies);

            var emitter = new MSBuildInstructionEmitter(LocalFileSystem.Instance)
            {
                    AllAssemblyReferenceFiles = Files(AllAssemblyReferenceFiles),
                    ContentFiles = Files(ContentFiles),
                    OpenWrapReferenceFiles = Files(OpenWrapReferenceFiles),
                    PdbFiles = Files(PdbFiles),
                    CodeDocFiles = Files(CodeDocFiles),
                    SatelliteAssemblies = Files(SatelliteAssemblies),
                    SerializationAssemblies = Files(SerializationAssemblies),
                    OutputAssemblyFiles = Files(OutputAssemblyFiles),
                    IncludePdbFiles = IncludePdbFiles,
                    IncludeCodeDocFiles = IncludeCodeDocFiles,
                    BasePath = BasePath,
                    ExportName = ExportName
            };
            foreach (var kv in emitter.GenerateInstructions())
                BuildEngine.LogMessageEvent(new BuildMessageEventArgs(
                                                         "[built(" +
                                                         kv.Key +
                                                         ", '" +
                                                         kv.Value +
                                                         "', " + 
                                                         AllowBinDuplicates.ToString().ToLowerInvariant() +
                                                         ")]",
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