﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using ICSharpCode.SharpZipLib.Zip;
using OpenFileSystem.IO;
using OpenWrap.Dependencies;

namespace OpenWrap.Repositories.NuPack
{
    public static class NuPackConverter
    {
        public const string NuSpecSchema = "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd";

        static Dictionary<string, string> FrameworkProfiles = new Dictionary<string, string>
        {
                { "NET", "net" },
                { ".NET", "net" },
                { "NETFramework", "net" },
                { ".NETFramework", "net" },
                { "SL", "sl" },
                { "Silverlight", "sl" }
        };

        static Dictionary<string, string> FrameworkVersions = new Dictionary<string, string>
        {
                { "2.0", "20"},
                { "3.0", "30"},
                { "3.5", "35"},
                { "4.0", "40"},
                { "20", "20"},
                { "30", "30"},
                { "35", "35"},
                { "40", "40"},
                { "40ClientProfile", "40cp"},
                { "35ClientProfile", "35cp"}
        };
        public static void Convert(Stream nuPackPackage, Stream openWrapPackage)
        {
            if (nuPackPackage == null) throw new ArgumentNullException("nuPackPackage");
            if (openWrapPackage == null) throw new ArgumentNullException("openWrapPackage");
            
            

            PackageBuilder.NewFromFiles(openWrapPackage, Content(nuPackPackage));
        }
        public static IEnumerable<PackageContent> Content(Stream nuPackPackage)
        {
            PackageContent content = null;
            string temporaryFile = null;
            try
            {
                if (!nuPackPackage.CanSeek)
                {
                    temporaryFile = Path.GetTempFileName();
                    using(var temporaryFileStream = File.OpenWrite(temporaryFile))
                        nuPackPackage.CopyTo(temporaryFileStream);

                    nuPackPackage = File.OpenRead(temporaryFile);
                }
                using (var inputZip = new ZipFile(nuPackPackage))
                {
                    foreach (var entry in inputZip.Cast<ZipEntry>().Where(x => x.IsFile))
                    {
                        var segments = entry.Name.Split('/');
                        if (segments.Length == 1 && Path.GetExtension(entry.Name).EqualsNoCase(".nuspec"))
                            yield return ConvertSpecification(inputZip, entry);
                        else if (segments.Length >= 2 && segments[0].Equals("lib", StringComparison.OrdinalIgnoreCase))
                            if ((content = ConvertAssembly(segments, inputZip, entry)) != null)
                                yield return content;
                    }
                }
            }
            finally
            {
                if (temporaryFile != null)
                {
                    nuPackPackage.Close();
                    File.Delete(temporaryFile);
                }
            }
        }


        static PackageContent ConvertAssembly(string[] segments, ZipFile inputZip, ZipEntry entry)
        {
            var destinationBinFolder = segments.Length == 2 ? "bin-net20" : ConvertAssemblyFolder(segments[1]);
            if (destinationBinFolder == null)
                return null;

            return new PackageContent
            {
                FileName = Path.GetFileName(entry.Name),
                RelativePath = destinationBinFolder,
                Stream = () => inputZip.GetInputStream(entry),
                Size = entry.Size
            };
        }

        static string ConvertAssemblyFolder(string identifier)
        {
            
            var nuPackProfile = FrameworkProfiles.Keys.FirstOrDefault(x => identifier.StartsWith(x, StringComparison.OrdinalIgnoreCase));

            var versionString = nuPackProfile == null ? identifier : identifier.Substring(nuPackProfile.Length);
            var nuPackVersion = FrameworkVersions.Keys.FirstOrDefault(x => versionString.Equals(x, StringComparison.OrdinalIgnoreCase));

            if (nuPackProfile != null)
                nuPackProfile = FrameworkProfiles[nuPackProfile];
            if (nuPackVersion != null)
                nuPackVersion = FrameworkVersions[nuPackVersion];
            return "bin-" + (nuPackProfile == null && nuPackVersion == null ? identifier : ((nuPackProfile ?? "net") + (nuPackVersion ?? "20")));
        }

        static PackageContent ConvertSpecification(ZipFile file, ZipEntry entry)
        {
            var nuspec = new XmlDocument();
            var ns = new XmlNamespaceManager(nuspec.NameTable);
            ns.AddNamespace("nuspec", NuSpecSchema);
            nuspec.Load(file.GetInputStream(entry));


            var descriptor = new PackageDescriptor
            {
                Name = nuspec.Element(XPaths.PackageName, ns),
                Version = nuspec.Element(XPaths.PackageVersion, ns).ToVersion(),
                Description = nuspec.Element(XPaths.packageDescrition, ns),
                Dependencies = nuspec.Elements(XPaths.PackageDependencies, ns).Select(x=>CreateDependency(x)).ToList()
            };
            var memoryStream = new MemoryStream();
            new PackageDescriptorReaderWriter().Write(descriptor, memoryStream);
            memoryStream.Position = 0;
            return new PackageContent
            {
                FileName = Path.GetFileNameWithoutExtension(entry.Name) + ".wrapdesc",
                RelativePath = ".",
                Size = memoryStream.Length,
                Stream = () =>
                {
                    memoryStream.Position = 0;
                    return memoryStream;
                }
            };
        }

        static PackageDependency CreateDependency(XmlNode xmlNode)
        {
            var dep = new PackageDependency
            {
                Name = (xmlNode.Attributes["id"] ?? xmlNode.Attributes["id", NuSpecSchema]).Value
            };
            var version = xmlNode.Attributes["version"] ?? xmlNode.Attributes["version", NuSpecSchema];
            var minversion = xmlNode.Attributes["minversion"] ?? xmlNode.Attributes["minversion", NuSpecSchema];
            var maxversion = xmlNode.Attributes["maxversion"] ?? xmlNode.Attributes["maxversion", NuSpecSchema];
            if (version != null)
                dep.VersionVertices.Add(new ExactVersionVertex(version.Value.ToVersion()));
            if (minversion != null)
                dep.VersionVertices.Add(new GreaterThenOrEqualVersionVertex(minversion.Value.ToVersion()));
            if (maxversion != null)
                dep.VersionVertices.Add(new LessThanVersionVertex(maxversion.Value.ToVersion()));
            return dep;
        }
    }
}
