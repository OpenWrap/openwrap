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
    public static class UserSettings
    {
        public static string UserRepositoryPath
        {
            get
            {
                return Path.Combine(
                    Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                        "openwrap"),
                        "wraps");
            }
        }

        public static RemoteRepositories RemoteRepositories
        {
            get { return WrapServices.GetService<IConfigurationManager>().RemoteRepositories(); }
        }
    }
}