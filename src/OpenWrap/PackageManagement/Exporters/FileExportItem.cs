using System;
using OpenFileSystem.IO;
using OpenWrap.PackageModel;

namespace OpenWrap.PackageManagement.Exporters
{
    public class FileExportItem : Exports.IFile
    {
        public FileExportItem(Path relativePath, IFile file, IPackage sourcePackage)
        {
            Path = relativePath.ToString();
            File = file;
        }

        public string Path { get; private set; }

        public IPackage Package { get; private set; }

        public IFile File { get; private set; }
    }
}