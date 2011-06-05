using System.Linq;
using OpenFileSystem.IO;
using OpenRasta.Client;
using OpenWrap.IO;

namespace OpenWrap.Repositories.FileSystem
{
    public class IndexedFolderRepositoryFactory : IRemoteRepositoryFactory
    {
        readonly IFileSystem _fileSystem;
        const string PREFIX = "[indexed-folder]";

        public IndexedFolderRepositoryFactory(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public IPackageRepository FromUserInput(string identifier)
        {
            if (!identifier.StartsWith("file://") && !identifier.StartsWith("indexed-folder://"))
            {
                return null;
            }
            var fileUri = identifier.ToUri();
            if (fileUri.Authority.EqualsNoCase("localhost") || string.IsNullOrEmpty(fileUri.Authority))
                identifier = fileUri.Segments.Skip(1)
                    .Select(_=>_.EndsWith("/") ? _.Substring(0, _.Length-1) : _)
                    .JoinString(System.IO.Path.DirectorySeparatorChar);
            else
                identifier = string.Format("{0}{0}{1}{0}{2}",
                                           System.IO.Path.DirectorySeparatorChar,
                                           fileUri.Authority,
                                           fileUri.Segments.JoinString(System.IO.Path.DirectorySeparatorChar));
            IDirectory directory;
            var file = _fileSystem.GetFile(identifier);

            if (file.Exists || file.Name == "index.wraplist")
                directory = file.Parent;
            else if ((directory = _fileSystem.GetDirectory(identifier)).Exists == false)
                directory.MustExist();

            var wrapList = directory.GetFile("index.wraplist");
            if (wrapList.Exists == false) wrapList.MustExist().WriteString("<package-list />");

            return new IndexedFolderRepository(directory.Name, directory){Token=PREFIX + directory.Path};
        }

        public IPackageRepository FromToken(string token)
        {
            if (token.StartsWith(PREFIX) == false)
                return null;
            var dir = _fileSystem.GetDirectory(token.Substring(PREFIX.Length));
            return new IndexedFolderRepository(dir.Name, dir) { Token = PREFIX + dir.Path };
        }
    }
}