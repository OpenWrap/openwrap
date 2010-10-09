using System;
using System.Text.RegularExpressions;

namespace OpenWrap.Dependencies
{
    public static class PackageNameUtility
    {
        private static readonly Regex VERSION_REGEX = new Regex(@"\-\d+(\.\d+(\.\d+(\.\d+)?)?)?$", RegexOptions.Compiled);
        public static string GetName(string name)
        {
            return GetVersion(name) == null ? name : name.Substring(0, name.LastIndexOf('-'));
        }
        public static Version GetVersion(string name)
        {
            var versionMAtch = VERSION_REGEX.Match(name);
            if (versionMAtch.Success)
                return new Version(name.Substring(name.LastIndexOf('-') + 1));
            return null;
        }
        public static string NormalizeFileName(string filename)
        {

            if (!filename.ToLowerInvariant().EndsWith(".wrap", StringComparison.OrdinalIgnoreCase))
                filename += ".wrap";
            return filename;
        }
        public static string PacakgeFileName(string packageName, string version)
        {
            return string.Format("{0}-{1}.wrap", packageName,version);
        }
    }
}