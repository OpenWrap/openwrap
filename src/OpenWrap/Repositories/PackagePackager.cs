using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystem.Local;

namespace OpenWrap.Repositories
{
    public class PackagePackager
    {
        public static void CreatePackage(IFile destinationPackage, string version, IEnumerable<IGrouping<string, IFile>> files)
        {
            using(var tempDirectory = LocalFileSystem.Instance.CreateTempDirectory())
            {

                foreach (var export in files)
                {
                    var folder = export.Key != "." ? tempDirectory.GetDirectory(export.Key).MustExist() : tempDirectory;
                    foreach(var file in export.Distinct())
                    {
                        var existingFile = folder.GetFile(file.Name);

                        if (existingFile.Exists)
                            existingFile.Delete();
                        file.CopyTo(folder);
                    }
                }
                // create version file
                using(var stream = tempDirectory.GetFile("version").MustExist().OpenWrite())
                    stream.Write(Encoding.UTF8.GetBytes(version));

                tempDirectory.ToZip(destinationPackage.Path);
            }
        }
    }
}
