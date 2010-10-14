using System;
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
                { "40", "40"}
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
                        if (segments.Length == 1 && Path.GetExtension(entry.Name).Equals(".nuspec", StringComparison.OrdinalIgnoreCase))
                            yield return ConvertSpecification(inputZip, entry);
                        else if (segments.Length > 2 && segments[0].Equals("lib", StringComparison.OrdinalIgnoreCase))
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
            var destinationBinFolder = ConvertAssemblyFolder(segments[1]);
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
            
            var profile = FrameworkProfiles.Keys.FirstOrDefault(x => identifier.StartsWith(x, StringComparison.OrdinalIgnoreCase));

            var nuPackFxVersion = profile == null ? identifier : identifier.Substring(profile.Length);
            var version = FrameworkVersions.Keys.FirstOrDefault(x => nuPackFxVersion.Equals(x, StringComparison.OrdinalIgnoreCase));

            if (profile == null && version == null) return null;
            return "bin-" + (profile ?? "net") + (version ?? "20");
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
                Description = nuspec.Element(XPaths.PacakgeDescrition, ns),
                Dependencies = nuspec.Elements(XPaths.PackageDependencies, ns).Select(CreateDependency).ToList()
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
    public static class XPaths
    {
        public const string Metadata = "package/metadata/";
        public const string MetadataNS = "nuspec:package/nuspec:metadata/";

        public const string Dependencies = "package/dependencies/dependency";
        public const string DependenciesNS = "nuspec:package/nuspec:dependencies/nuspec:dependency";

        public static string[] PackageName = new[] { Metadata + "id", MetadataNS + "nuspec:id" };
        public static string[] PackageVersion = new[] { Metadata + "version", MetadataNS + "nuspec:version" };
        public static string[] PacakgeDescrition = new[] { Metadata + "description", MetadataNS + "nuspec:description" };

        public static string[] PackageDependencies = new[] { Dependencies, DependenciesNS };
    }
    public static class XmlDocumentExtensions
    {
        public static string Element(this XmlDocument document, string[] xpaths, XmlNamespaceManager ns)
        {
            return xpaths.Select(x => document.SelectSingleNode(x, ns)).NotNull().Select(x => x.InnerText).FirstOrDefault();
        }
        public static IEnumerable<XmlNode> Elements(this XmlDocument document, string[] xpaths, XmlNamespaceManager ns)
        {
            return xpaths.SelectMany(x => document.SelectNodes(x, ns).OfType<XmlNode>());
        }
    }
}
