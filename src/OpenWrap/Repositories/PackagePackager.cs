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
<<<<<<< HEAD
        public static void CreatePackage(IFile destinationPackage, IEnumerable<IGrouping<string, IFile>> files)
=======
        public static void CreatePackage(IFile destinationPackage, string version, IEnumerable<IGrouping<string, IFile>> files)
>>>>>>> f73f0dce227b51986ff52ba6fb3db3b2a48c748f
        {
            using(var tempDirectory = LocalFileSystem.Instance.CreateTempDirectory())
            {

                foreach (var export in files)
                {
<<<<<<< HEAD
                    var folder = tempDirectory.GetDirectory(export.Key).MustExist();
                    foreach(var file in export)
                    {
                        file.CopyTo(tempDirectory);
                    }
                }
=======
                    var folder = export.Key != "." ? tempDirectory.GetDirectory(export.Key).MustExist() : tempDirectory;
                    foreach(var file in export.Distinct())
                    {
                        file.CopyTo(folder);
                    }
                }
                // create version file
                using(var stream = tempDirectory.GetFile("version").MustExist().OpenWrite())
                    stream.Write(Encoding.UTF8.GetBytes(version));

>>>>>>> f73f0dce227b51986ff52ba6fb3db3b2a48c748f
                tempDirectory.ToZip(destinationPackage.Path);
            }
        }
    }
}
