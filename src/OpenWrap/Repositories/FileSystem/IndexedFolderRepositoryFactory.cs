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
                    .Select(_ => _.EndsWith("/") ? _.Substring(0, _.Length - 1) : _)
                    .JoinString(System.IO.Path.DirectorySeparatorChar);
            else
                identifier = string.Format("{0}{0}{1}{2}",
                                           System.IO.Path.DirectorySeparatorChar,
                                           fileUri.Authority,
                                           fileUri.Segments.Select(_ => _.EndsWith("/") ? _.Substring(0, _.Length - 1) : _).JoinString(System.IO.Path.DirectorySeparatorChar));
            IDirectory directory;
            var file = _fileSystem.GetFile(identifier);
            if (fileUri.Scheme == "file" && file.Exists && file.Name != "index.wraplist")
                return null;
            if (file.Exists || file.Name == "index.wraplist")
            {
                directory = file.Parent;
                identifier = System.IO.Path.GetDirectoryName(directory.Path);
            }
            else if ((directory = _fileSystem.GetDirectory(identifier)).Exists == false)
                directory.MustExist();
            var wrapList = directory.GetFile("index.wraplist");

            // if there are existing files in a folder but no wraplist, not an indexed folder
            if (fileUri.Scheme == "file" && wrapList.Exists == false && directory.Files().Where(_ => _.Name != "index.wraplist").Any())
                return null;

            if (wrapList.Exists == false) wrapList.MustExist().WriteString("<package-list />");
            // TODO: Fix the Path problem in OFS (see https://api.github.com/repos/openrasta/openfilesystem/issues/8)
            // Right now this code will break mono for UNC paths (although they're not supported anyway)
            var fullPath = wrapList.Path.FullPath;
            if (string.IsNullOrEmpty(fileUri.Authority) == false && fileUri.Authority.EqualsNoCase("localhost") == false && !fullPath.StartsWith("\\\\"))
                fullPath = string.Format("{0}{0}{1}",System.IO.Path.DirectorySeparatorChar, fullPath);
            return new IndexedFolderRepository(directory.Name, directory) { Token = PREFIX + fullPath };
        }

        public IPackageRepository FromToken(string token)
        {
            if (token.StartsWith(PREFIX) == false)
                return null;
            var file = _fileSystem.GetFile(token.Substring(PREFIX.Length));
            return new IndexedFolderRepository(file.Name, file.Parent) { Token = PREFIX + file.Path };
        }
    }
    
}