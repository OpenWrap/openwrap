﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using OpenFileSystem.IO;
using OpenWrap.Collections;
using Path = System.IO.Path;

namespace OpenWrap.IO.Packaging
{
    public static class Packager
    {
        public static void NewFromFiles(IFile destinationPackage, IEnumerable<PackageContent> content)
        {
            using (var wrapStream = destinationPackage.OpenWrite())
                NewFromFiles(wrapStream, content);
        }

        public static void NewFromFiles(Stream wrapStream, IEnumerable<PackageContent> content)
        {
            using (var zipFile = new ZipOutputStream(wrapStream) { IsStreamOwner = false, UseZip64 = UseZip64.Off })
            {
                foreach (var contentFile in content)
                {
                    zipFile.PutNextEntry(GetZipEntry(contentFile));

                    using (var contentStream = contentFile.Stream())
                        contentStream.CopyTo(zipFile);
                }
                zipFile.Finish();
            }
        }

        public static IFile NewWithDescriptor(IFile wrapFile, string name, string version, params string[] descriptorLines)
        {
            return NewWithDescriptor(wrapFile, name, version, Enumerable.Empty<PackageContent>(), descriptorLines);
        }

        public static IFile NewWithDescriptor(IFile wrapFile, string name, string version, IEnumerable<PackageContent> addedContent, params string[] descriptorLines)
        {
            var lines = descriptorLines.ToList();
            if (lines.None(_ => _.StartsWithNoCase("name:")))
                lines.Add("name: " + name);
            if (lines.None(_=>_.StartsWithNoCase("semantic-version:")))
                lines.Add("semantic-version: " + version);
            if (lines.None(_ => _.StartsWithNoCase("version:")))
                lines.Add("version: " + version.ToSemVer().ToVersion());

            var descriptorContent = lines.JoinString("\r\n").ToUTF8Stream();
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

        static ZipEntry GetZipEntry(PackageContent contentFile)
        {
            var target = string.IsNullOrEmpty(contentFile.RelativePath) ? "." : contentFile.RelativePath;
            if (target == ".")
                return new ZipEntry(Path.GetFileName(contentFile.FileName));
            if (target.Last() != '/')
                target += '/';
            var fileEntry = new ZipEntry(Path.Combine(target, contentFile.FileName));
            if (contentFile.Size != null)
                fileEntry.Size = contentFile.Size.Value;
            return fileEntry;
        }
    }
}