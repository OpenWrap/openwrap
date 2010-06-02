using System;
using System.IO;
using System.Xml;
using OpenWrap.Repositories;

namespace OpenWrap.Repositories
{
    public static class UserRepository
    {

        static UserRepository()
        {
            if (!Directory.Exists(UserSettings.UserRepositoryPath))
                Directory.CreateDirectory(UserSettings.UserRepositoryPath);

            Current = new FolderRepository(UserSettings.UserRepositoryPath);
        }

        public static IPackageRepository Current
        {
            get; private set;
        }
    }
}