using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using OpenFileSystem.IO;

namespace OpenWrap.Dependencies
{
    public static class PackageBuilder
    {
        public static void NewFromFiles(IFile destinationPackage, IEnumerable<PackageContent> content)
        {
            using (var wrapStream = destinationPackage.OpenWrite())
                NewFromFiles(wrapStream, content);
        }

        public static void NewFromFiles(Stream wrapStream, IEnumerable<PackageContent> content)
        {
            using (var zipFile = new ZipOutputStream(wrapStream){IsStreamOwner = false, UseZip64 = UseZip64.Off})
            {
                foreach(var contentFile in content)
                {
                    zipFile.PutNextEntry(GetZipEntry(contentFile));

                    using (var contentStream = contentFile.Stream())
                        contentStream.CopyTo(zipFile);
                }
                zipFile.Finish();
            }
        }

        static ZipEntry GetZipEntry(PackageContent contentFile)
        {
            if (contentFile.RelativePath == ".")
                return new ZipEntry(System.IO.Path.GetFileName(contentFile.FileName));
            var target = contentFile.RelativePath;
            if (target.Last() != '/')
                target += '/';
            var fileEntry = new ZipEntry(System.IO.Path.Combine(target, contentFile.FileName));
            if (contentFile.Size != null)
                fileEntry.Size = contentFile.Size.Value;
            return fileEntry;
        }

        public static IFile NewWithDescriptor(IFile wrapFile, string name, string version, params string[] descriptorLines)
        {
            return NewWithDescriptor(wrapFile, name, version, Enumerable.Empty<PackageContent>(), descriptorLines);
        }

        public static IFile NewWithDescriptor(IFile wrapFile, string name, string version, IEnumerable<PackageContent> addedContent, params string[] descriptorLines)
        {
            var descriptorContent = (descriptorLines.Any() ? String.Join("\r\n", descriptorLines) : " ").ToUTF8Stream();
            var versionContent = version.ToUTF8Stream();
            var content = new List<PackageContent>
            {
                    new PackageContent
                    {
                            FileName = name + ".wrapdesc",
                            RelativePath = ".",
                            Size = descriptorContent.Length,
                            Stream = () =>
                            { 
                                descriptorContent.Position = 0;
                                return descriptorContent;
                            }
                    },
                    new PackageContent
                    {
                            FileName = "version",
                            RelativePath = ".",
                            Size = versionContent.Length,
                            Stream = () =>
                            {
                                versionContent.Position = 0;
                                return versionContent;
                            }
                    }
            }.Concat(addedContent);
            NewFromFiles(wrapFile, content);
            return wrapFile;
        }
    }
    public class PackageContent
    {
        public string RelativePath { get; set; }
        public string FileName { get; set; }
        public Func<Stream> Stream { get;set; }

        public long? Size { get; set; }
    }
}