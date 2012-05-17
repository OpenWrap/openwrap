using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using OpenWrap.Collections;
using OpenWrap.PackageModel;

namespace OpenWrap.Repositories.NuGet
{
    public static class NuConverter
    {
        public static IEnumerable<VersionVertex> ConvertNuGetVersionRange(string value)
        {
            if (string.IsNullOrEmpty(value))
                yield break;
            var simpleVersion = value.ToSemVer();
            if (simpleVersion != null)
            {
                // play around with version ranges to account for NuGet not making distinction between
                // revisions and implementation details of their tool algorithm.
                if (simpleVersion.Patch == -1)
                {
                    yield return new EqualVersionVertex(simpleVersion);
                }
                else
                {
                    var lowerBound = RemoveInsignificantVersionNumbers(value);
                    var upperBound = new SemanticVersion(lowerBound.Major, lowerBound.Minor + 1);
                    yield return new GreaterThanOrEqualVersionVertex(lowerBound);
                    yield return new LessThanVersionVertex(upperBound);
                }
                yield break;
            }
            SemanticVersion beginVersion = null;
            SemanticVersion endVersion = null;
            StringBuilder currentIdentifier = new StringBuilder();
            bool inclusiveBeginning = false;
            bool inclusiveEnding = false;
            bool endIncluded = false;
            Action clearVer = () => currentIdentifier = new StringBuilder();
            foreach (var ch in value)
            {
                if (ch == ' ') continue;
                if (ch == '[')
                {
                    inclusiveBeginning = true;
                    clearVer();
                }
                else if (ch == '(')
                {
                    inclusiveBeginning = false;
                    clearVer();
                }
                else if (ch == ']')
                {
                    inclusiveEnding = true;
                }
                else if (ch == ')')
                {
                    inclusiveEnding = false;
                }
                else if ((ch >= '0' && ch <= '9') || ch == '.')
                {
                    currentIdentifier.Append(ch);
                }
                else if (ch == ',')
                {
                    if (currentIdentifier.Length > 0)
                    {
                        beginVersion = RemoveInsignificantVersionNumbers(currentIdentifier);
                        clearVer();
                    }
                    endIncluded = true;
                }
            }
            if (currentIdentifier.Length > 0 && endIncluded)
                endVersion = currentIdentifier.ToString().ToSemVer();
            else if (currentIdentifier.Length > 0 && endIncluded == false && beginVersion == null)
                beginVersion = currentIdentifier.ToString().ToSemVer();


            if (beginVersion != null && !endIncluded)
            {
                if (inclusiveBeginning && inclusiveEnding)
                    yield return new EqualVersionVertex(beginVersion);
                yield break;
            }
            if (beginVersion != null && inclusiveBeginning)
                yield return new GreaterThanOrEqualVersionVertex(beginVersion);
            else if (beginVersion != null && inclusiveBeginning == false)
                yield return new GreaterThanVersionVertex(beginVersion);

            if (endVersion != null && inclusiveEnding == false)
                yield return new LessThanVersionVertex(endVersion);
            else if (endVersion != null && inclusiveEnding)
                yield return new LessThanOrEqualVersionVertex(endVersion);
        }

        public static PackageDescriptor ConvertSpecificationToDescriptor(XmlDocument nuspec)
        {
            var ns = new XmlNamespaceManager(nuspec.NameTable);

            ns.AddNamespace("nuspec", NuGetConverter.NuSpecSchema);


            var descriptor = new PackageDescriptor
            {
                    Name = nuspec.Element(XPaths.PackageName, ns),
                    SemanticVersion = nuspec.Element(XPaths.PackageVersion, ns).ToSemVer(),
                    Description = StripLines(nuspec.Element(XPaths.PackageDescription, ns))
            };
            descriptor.Dependencies.AddRange(
                    nuspec.Elements(XPaths.PackageDependencies, ns).Select(CreateDependency)
                    );
            return descriptor;
        }

        static PackageDependency CreateDependency(XmlNode xmlNode)
        {
            var dep = new PackageDependencyBuilder((xmlNode.Attributes["id"] ?? xmlNode.Attributes["id", NuGetConverter.NuSpecSchema]).Value);

            var version = xmlNode.Attributes["version"] ?? xmlNode.Attributes["version", NuGetConverter.NuSpecSchema];
            var minversion = xmlNode.Attributes["minversion"] ?? xmlNode.Attributes["minversion", NuGetConverter.NuSpecSchema];
            var maxversion = xmlNode.Attributes["maxversion"] ?? xmlNode.Attributes["maxversion", NuGetConverter.NuSpecSchema];
            if (minversion != null || maxversion != null)
            {
                if (minversion != null)
                    dep.VersionVertex(new GreaterThanOrEqualVersionVertex(minversion.Value.ToSemVer()));
                if (maxversion != null)
                    dep.VersionVertex(new LessThanVersionVertex(maxversion.Value.ToSemVer()));
            }
            else
            {
                dep.SetVersionVertices(ConvertNuGetVersionRange(version != null ? version.Value : null).DefaultIfEmpty(new AnyVersionVertex()));
            }
            return dep;
        }

        static SemanticVersion RemoveInsignificantVersionNumbers(string versionString)
        {
            if (versionString == null)
                return null;

            var ver = versionString.ToSemVer();
            if (ver == null)
                return null;
            return new SemanticVersion(ver.Major, ver.Minor);
        }

        static SemanticVersion RemoveInsignificantVersionNumbers(StringBuilder currentIdentifier)
        {
            var versionString = currentIdentifier.ToString();
            return RemoveInsignificantVersionNumbers(versionString);
        }

        static string StripLines(string element)
        {
            if (String.IsNullOrEmpty(element))
                return element;
            var noLineBreaks = element.Replace("\r\n", " ").Replace("\n", " ");
            while (noLineBreaks.Contains("  "))
                noLineBreaks = noLineBreaks.Replace("  ", " ");
            return noLineBreaks;
        }
    }
}