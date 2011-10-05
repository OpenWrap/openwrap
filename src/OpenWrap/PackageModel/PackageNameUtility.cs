using System;
using System.Text.RegularExpressions;

namespace OpenWrap.PackageModel
{
    public static class PackageNameUtility
    {
        static readonly Regex VERSION_REGEX = new Regex(@"\-(?<version>\d+(\.\d+(\.\d+(\.\d+)?)?)?)(\.wrap)?$", RegexOptions.Compiled);

        // TODO: Remove remove remove, file name matching is evil
        public static string GetName(string name)
        {
            return GetVersion(name) == null ? name : name.Substring(0, name.LastIndexOf('-'));
        }
        // TODO: Remove remove remove, file name matching is evil
        public static Version GetVersion(string name)
        {
            var versionMAtch = VERSION_REGEX.Match(name);
            if (versionMAtch.Success)
                return versionMAtch.Groups["version"].Value.ToVersion();
            return null;
        }

        public static string NormalizeFileName(string filename)
        {
            if (!filename.ToLowerInvariant().EndsWith(".wrap", StringComparison.OrdinalIgnoreCase))
                filename += ".wrap";
            return filename;
        }

        public static string PackageFileName(string packageName, string version)
        {
            return string.Format("{0}-{1}.wrap", packageName, version);
        }
    }
}