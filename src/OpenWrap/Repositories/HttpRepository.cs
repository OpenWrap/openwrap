using System;
using System.IO;
using System.Xml;
using OpenRasta.Wrap.Sources;

namespace OpenWrap.Repositories
{
    public static class UserRepository
    {
        static readonly string _userWrapsPath = FileSystem.CombinePaths(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "OpenWrap", "wraps");

        static UserRepository()
        {
            if (!Directory.Exists(_userWrapsPath))
                Directory.CreateDirectory(_userWrapsPath);

            Current = new FolderRepository(_userWrapsPath);
        }

        public static IWrapRepository Current
        {
            get; private set;
        }
    }
}