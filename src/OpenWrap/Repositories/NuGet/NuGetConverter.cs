using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using ICSharpCode.SharpZipLib.Zip;
using OpenWrap.IO;
using OpenWrap.IO.Packaging;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Serialization;

namespace OpenWrap.Repositories.NuGet
{
    public static class NuGetConverter
    {
        public const string NuSpecSchema = "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd";
        // TODO: Move all framework profile stuff into FrameworkVersioning.cs
        static readonly Dictionary<string, string> FrameworkProfiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
                { "NET", "net" },
                { ".NET", "net" },
                { "NETFramework", "net" },
                { ".NETFramework", "net" },
                { "SL", "sl" },
                { "Silverlight", "sl" }
        };

        static readonly Dictionary<string, string> FrameworkVersions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
                { "2.0", "20" },
                { "3.0", "30" },
                { "3.5", "35" },
                { "4.0", "40" },
                { "20", "20" },
                { "30", "30" },
                { "35", "35" },
                { "40", "40" },

                { "40ClientProfile", "40cp" },
                { "35ClientProfile", "35cp" },
                
                { "40-client", "40cp" },
                { "35-client", "35cp" },
                
                { "40-Full", "40" },
                { "35-Full", "35" }
        };

        public static IEnumerable<PackageContent> Content(Stream nuGetPackage)
        {
            PackageContent content = null;
            string temporaryFile = null;
            try
            {
                if (!nuGetPackage.CanSeek)
                {
                    temporaryFile = Path.GetTempFileName();
                    using (var temporaryFileStream = File.OpenWrite(temporaryFile))
                        nuGetPackage.CopyTo(temporaryFileStream);

                    nuGetPackage = File.OpenRead(temporaryFile);
                }
                using (var inputZip = new ZipFile(nuGetPackage))
                {
                    foreach (var entry in inputZip.Cast<ZipEntry>().Where(x => x.IsFile))
                    {
                        var segments = entry.Name.Split('/');
                        if (segments.Length == 1 && Path.GetExtension(entry.Name).EqualsNoCase(".nuspec"))
                            yield return ConvertSpecification(inputZip, entry);
                        else if (segments.Length >= 2 && segments[0].EqualsNoCase("lib"))
                            if ((content = ConvertAssembly(segments, inputZip, entry)) != null)
                                yield return content;
                    }
                }
            }
            finally
            {
                if (temporaryFile != null)
                {
                    nuGetPackage.Close();
                    File.Delete(temporaryFile);
                }
            }
        }

        public static void Convert(Stream nuGetPackage, Stream openWrapPackage)
        {
            if (nuGetPackage == null) throw new ArgumentNullException("nuGetPackage");
            if (openWrapPackage == null) throw new ArgumentNullException("openWrapPackage");


            Packager.NewFromFiles(openWrapPackage, Content(nuGetPackage));
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
            var nugetProfile = FrameworkProfiles.Keys.FirstOrDefault(x => identifier.StartsWith(x, StringComparison.OrdinalIgnoreCase));

            var versionString = nugetProfile == null ? identifier : identifier.Substring(nugetProfile.Length);
            var nugetVersion = FrameworkVersions.Keys.FirstOrDefault(x => versionString.EqualsNoCase(x));

            if (nugetProfile != null)
                nugetProfile = FrameworkProfiles[nugetProfile];
            if (nugetVersion != null)
                nugetVersion = FrameworkVersions[nugetVersion];
            return "bin-" + (nugetProfile == null && nugetVersion == null ? identifier : ((nugetProfile ?? "net") + (nugetVersion ?? "20")));
        }

        static PackageContent ConvertSpecification(ZipFile file, ZipEntry entry)
        {
            var inputStream = file.GetInputStream(entry);

            var nuspec = new XmlDocument();

            nuspec.Load(inputStream);

            PackageDescriptor descriptor = NuConverter.ConvertSpecificationToDescriptor(nuspec);
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
    }
}