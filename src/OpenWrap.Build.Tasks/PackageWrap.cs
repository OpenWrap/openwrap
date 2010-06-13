using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using OpenWrap.IO;

namespace OpenWrap.Build.Tasks
{
    public class PackageWrap : Task
    {
        readonly IFileSystem _fileSystem;

        public PackageWrap() : this(FileSystem.Local)
        {
            
        }

        public PackageWrap(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        [Required]
        public string Destination { get; set; }
        [Required]
        public ITaskItem[] Files { get; set; }

        public override bool Execute()
        {
            using(var tempDirectory = _fileSystem.CreateTempDirectory())
            try
            {
                CopyFilesToTempDirectory(tempDirectory);

                var packageFullPath = _fileSystem.GetPath(Destination);
                var zipFile = tempDirectory.ToZip(packageFullPath);

                Log.LogMessage("Created package at {0}", packageFullPath);
            }
            catch(Exception e)
            {
                Log.LogErrorFromException(e);
                return false;
            }
            return true;
        }

        void CopyFilesToTempDirectory(ITemporaryDirectory tempDirectory)
        {
            foreach (var export in Files.GroupBy(file => file.GetMetadata("Export")))
            {
                var exportFolder = tempDirectory.GetDirectory(export.Key).Create();
                foreach (var file in export)
                {
                    var fileToCopy = _fileSystem.GetFile(file.ItemSpec);
                    if (fileToCopy == null)
                    {
                        Log.LogWarning("File '{0}' not found.", file.ItemSpec);
                    }
                    else
                    {
                        fileToCopy.CopyTo(exportFolder);
                        Log.LogMessage("Copying file '{0}' to '{1}'.", fileToCopy, exportFolder);
                    }
                }
            }
        }
    }
}
