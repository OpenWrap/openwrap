// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using OpenFileSystem.IO.FileSystems.Local;
using OpenWrap.Collections;

namespace OpenWrap.Build.Tasks
{
    public class PublishPackageContent : Task
    {
        public bool AllowBinDuplicates { get; set; }
        public ITaskItem[] BuildOutputFiles { get; set; }
        public ITaskItem[] ContentFiles { get; set; }
        public string ExportName { get; set; }
        public bool IncludeBinFiles { get; set; }

        public bool IncludeCodeDocFiles { get; set; }
        public bool IncludeContentFiles { get; set; }
        public bool IncludePdbFiles { get; set; }
        public bool IncludeSourceFiles { get; set; }
        public string RootPath { get; set; }
        public ITaskItem[] SourceFiles { get; set; }

        public override bool Execute()
        {
            WriteLow("Include Binaries: " + IncludeBinFiles);
            WriteLow("Include Content: " + IncludeContentFiles);
            WriteLow("Include XML Documentation: " + IncludeCodeDocFiles);
            WriteLow("Include PDBs: " + IncludePdbFiles);
            WriteLow("Include Source: " + IncludeSourceFiles);
            WriteLow("Export to: " + ExportName);
            WriteLow("Allow duplicates: " + AllowBinDuplicates);

            WriteLow("Root path: " + RootPath);

            WriteFiles("BuildOutput", BuildOutputFiles);
            WriteFiles("ContentFiles", ContentFiles);
            WriteFiles("SourceFiles", SourceFiles);

            var emitter = new MSBuildInstructionEmitter(LocalFileSystem.Instance)
            {
                ContentFiles = Files(ContentFiles),
                SourceFiles = Files(SourceFiles),
                BuildOutputFiles = Files(BuildOutputFiles),
                IncludePdbFiles = IncludePdbFiles,
                IncludeCodeDocFiles = IncludeCodeDocFiles,
                IncludeSourceFiles = IncludeSourceFiles,
                IncludeBinFiles = IncludeBinFiles,
                IncludeContentFiles = IncludeContentFiles,
                ExportName = ExportName,
                RootPath = RootPath
            };
            foreach (var kv in emitter.GenerateInstructions())
                BuildEngine.LogMessageEvent(new BuildMessageEventArgs(
                                                string.Format("[built({0}, '{1}', {2})]", kv.Key, kv.Value, AllowBinDuplicates.ToString().ToLowerInvariant()),
                                                null,
                                                "OpenWrap",
                                                MessageImportance.High));
            return true;
        }

        static ILookup<string, string> Files(IEnumerable<ITaskItem> specs)
        {
            return specs == null
                       ? Lookup<string, string>.Empty
                       : specs.NotNull()
                             .Select(x => new { target = GetTarget(x), path = Path.GetFullPath(x.ItemSpec) })
                             .ToLookup(_ => _.target, _ => _.path);
        }

        static string GetTarget(ITaskItem x)
        {
            var target = x.GetMetadata("TargetPath");
            if (Path.GetFileName(x.ItemSpec) == Path.GetFileName(target))
                target = Path.GetDirectoryName(target);
            return target;
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
    }
}

// ReSharper restore UnusedAutoPropertyAccessor.Global
// ReSharper restore MemberCanBePrivate.Global