using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using OpenFileSystem.IO;

namespace OpenWrap.Dependencies
{
    public static class PackageBuilder
    {

        public static IFile New(IFile wrapFile, string name, string version, params string[] descriptorLines)
        {
            //var wrapFile = new InMemoryFile(name + "-" + version + ".wrap");
            using (var wrapStream = wrapFile.OpenWrite())
            using (var zipFile = new ZipOutputStream(wrapStream))
            {
                var nameTransform = new ZipNameTransform();

                zipFile.PutNextEntry(new ZipEntry(name + ".wrapdesc"));

                var descriptorContent = descriptorLines.Any()
                                                ? string.Join("\r\n", descriptorLines)
                                                : " ";
                    
                zipFile.Write(Encoding.UTF8.GetBytes(descriptorContent));

                var versionEntry = new ZipEntry("version");
                zipFile.PutNextEntry(versionEntry);

                var versionData = Encoding.UTF8.GetBytes(version);
                zipFile.Write(versionData);
                zipFile.Finish();
            }
            return wrapFile;
        }
    }
}