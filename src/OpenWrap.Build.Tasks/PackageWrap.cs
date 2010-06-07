using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace OpenWrap.Build.Tasks
{
    public class PackageWrap : Task
    {
        [Required]
        public string Destination { get; set; }
        [Required]
        public ITaskItem[] Files { get; set; }
        public override bool Execute()
        {
            var tempFolder = Path.Combine(Path.GetTempPath(),Path.GetRandomFileName());
            try
            {
                //Debugger.Launch();
                Directory.CreateDirectory(tempFolder);

                foreach (var export in Files.GroupBy(file => file.GetMetadata("Export")))
                {
                    var exportFolder = Path.Combine(tempFolder, export.Key);
                    if (!Directory.Exists(exportFolder))
                        Directory.CreateDirectory(exportFolder);
                    foreach (var file in export)
                    {
                        var filePath = Path.GetFullPath(file.ItemSpec);
                        File.Copy(filePath, Path.Combine(exportFolder, Path.GetFileName(filePath)), true);
                        Log.LogMessage("Copying file '{0}' to '{1}'.", filePath, exportFolder);
                    }
                }
                var packageFullPath = Path.GetFullPath(Destination);
                if (File.Exists(packageFullPath))
                    File.Delete(packageFullPath);
                if (!Directory.Exists(Path.GetDirectoryName(packageFullPath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(packageFullPath));
                new FastZip().CreateZip(packageFullPath, tempFolder, true, "");
                Log.LogMessage("Created package at {0}", packageFullPath);
            }
            catch(Exception e)
            {
                Debugger.Launch();
                Directory.Delete(tempFolder, true);
                Log.LogErrorFromException(e);
                return false;
            }
            return true;
        }
    }
}
