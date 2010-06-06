using System;
using System.Collections.Generic;
using OpenWrap.Repositories;

namespace OpenWrap
{
    public static class UserSettings
    {
        static IEnumerable<Uri> _remoteRepositoriesPaths = new[]{ new Uri("http://localhost:42",UriKind.Absolute) };


        public static string UserRepositoryPath { get { return FileSystem.CombinePaths(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "openwrap", "wraps"); } }

        public static IEnumerable<Uri> RemoteRepositories
        {
            get { return _remoteRepositoriesPaths; }
            set { _remoteRepositoriesPaths = value; }
        }
    }
}