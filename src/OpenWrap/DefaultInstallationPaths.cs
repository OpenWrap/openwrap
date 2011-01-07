using System;
using System.IO;

namespace OpenWrap
{
    public static class DefaultInstallationPaths
    {
        public static string ConfigurationDirectory
        {
            get { return Path.Combine(SystemDirectory, "config"); }
        }

        public static string SystemDirectory
        {
            get
            {
                return
                        Path.Combine(
                                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                "openwrap");
            }
        }

        public static string SystemRepositoryDirectory
        {
            get { return Path.Combine(SystemDirectory, "wraps"); }
        }
    }}