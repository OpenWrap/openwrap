using System;
using System.Text.RegularExpressions;

namespace OpenWrap.PackageModel
{
    public static class PackageNameUtility
    {
        public const string NAME_CHARS = "[a-zA-Z0-9-\\.\\+]";

        public const string VERSION_SEGMENTS_CHARS = "[a-zA-Z0-9-\\.]";
        const string COMPONENT_NAME = @"(?<name>" + NAME_CHARS + @"+)";

        const string COMPONENT_VER_NET = @"\.\d+";

        const string COMPONENT_VER_SEMVER =
            "(\\-" + VERSION_SEGMENTS_CHARS + "+)?" +
            "(\\+" + VERSION_SEGMENTS_CHARS + "+)?";

        static readonly Regex FILE_NAME_REGEX = new Regex(
                @"^" +
                COMPONENT_NAME +
                "(\\~(?<flavour>[a-zA-Z0-9+]+))?" +
                "\\-(?<version>\\d+\\.\\d+(\\.\\d+(" +
                COMPONENT_VER_NET + "|" + COMPONENT_VER_SEMVER +
                ")?)?)" +
                "(\\.wrap)?$"
            );

        // TODO: Remove remove remove, file name matching is evil
        public static string GetName(string name)
        {
            var versionMAtch = FILE_NAME_REGEX.Match(name);

            if (versionMAtch.Success)
                return versionMAtch.Groups["name"].Value;
            return null;
        }

        // TODO: Remove remove remove, file name matching is evil
        public static SemanticVersion GetVersion(string name)
        {
            var versionMAtch = FILE_NAME_REGEX.Match(name);
            if (versionMAtch.Success)
                return versionMAtch.Groups["version"].Value.ToSemVer();
            return null;
        }

        public static bool IsNameValid(string name)
        {
            return Regex.IsMatch(name, "^" + NAME_CHARS + "+$");
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