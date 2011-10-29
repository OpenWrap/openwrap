using System;
using System.Linq;
using System.Net;
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

        public IPackageRepository FromUserInput(string userInput, NetworkCredential crendentials = null)
        {
            if (!userInput.StartsWith("file://") && !userInput.StartsWith("indexed-folder://"))
            {
                return null;
            }
            
            var fileUri = userInput.ToUri();

            var directoryPath = fileUri.ToPath();

            IDirectory directory;
            var wrapFile = _fileSystem.GetFile(directoryPath.FullPath);
            if (IsFileUriPontingToUnknownFileName(fileUri, wrapFile))
                return null;
            if (wrapFile.Exists || wrapFile.Name == "index.wraplist")
                directory = wrapFile.Parent;
            else if ((directory = _fileSystem.GetDirectory(directoryPath.FullPath)).Exists == false)
                directory.MustExist();
            wrapFile = directory.GetFile("index.wraplist");

            // if there are existing files in a folder but no wraplist, not an indexed folder
            if (FolderNotEmptyAndMissingWraplist(fileUri, directory, wrapFile))
                return null;

            if (wrapFile.Exists == false) wrapFile.MustExist().WriteString("<package-list />");

            return new IndexedFolderRepository(directory.Name, directory) { Token = PREFIX + wrapFile.Path.ToUri("indexed-folder") };
        }


        static bool FolderNotEmptyAndMissingWraplist(Uri fileUri, IDirectory directory, IFile wrapList)
        {
            return fileUri.Scheme == "file" && wrapList.Exists == false && directory.Files().Where(_ => _.Name != "index.wraplist").Any();
        }

        static bool IsFileUriPontingToUnknownFileName(Uri fileUri, IFile file)
        {
            return fileUri.Scheme == "file" && file.Exists && file.Name != "index.wraplist";
        }

        public IPackageRepository FromToken(string token)
        {
            if (token.StartsWith(PREFIX) == false)
                return null;

            var path = token.Substring(PREFIX.Length);
            IFile file = null;
                Uri uri;
                if (Uri.TryCreate(path, UriKind.Absolute, out uri))
                    file = _fileSystem.GetFile(uri.ToPath().ToString());
            if (file == null)
                    file = _fileSystem.GetFile(path);

            return new IndexedFolderRepository(file.Name, file.Parent) { Token = PREFIX + file.Path.ToUri("indexed-folder") };
        }
    }
}