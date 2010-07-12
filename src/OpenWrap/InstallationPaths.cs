using System;
using System.Collections.Generic;
using System.IO;
using OpenWrap.Configuration;
using OpenWrap.Configuration.remote_repositories;
using OpenWrap.IO;
using OpenWrap.Repositories;
using OpenWrap.Services;

namespace OpenWrap
{
    public static class InstallationPaths
    {
        public static string UserRepositoryPath
        {
            get
            {
                return Path.Combine(RootPath, "wraps");
            }
        }
        public static string ConfigurationDirectory
        {
            get
            {
                return Path.Combine(RootPath, "config");
            }
        }
        public static string RootPath
        {
            get
            {
                return 
                    Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "openwrap");
            }
        }
    }
}