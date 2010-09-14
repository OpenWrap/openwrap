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
        public static void CreatePackage(IFile destinationPackage, IEnumerable<IGrouping<string, IFile>> files)
        {
            using(var tempDirectory = LocalFileSystem.Instance.CreateTempDirectory())
            {

                foreach (var export in files)
                {
                    var folder = tempDirectory.GetDirectory(export.Key).MustExist();
                    foreach(var file in export)
                    {
                        file.CopyTo(tempDirectory);
                    }
                }
                tempDirectory.ToZip(destinationPackage.Path);
            }
        }
    }
}
