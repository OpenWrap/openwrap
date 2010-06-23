using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using OpenWrap.IO;
using OpenWrap.IO.FileSystem.Local;

namespace OpenWrap.Repositories
{
    internal class ZipWrapperFile : IFile
    {
        readonly ZipFile _zip;
        readonly ZipEntry _entry;

        public ZipWrapperFile(ZipFile zip, ZipEntry entry)
        {
            _zip = zip;
            _entry = entry;
        }

        public IFile Create()
        {
            throw new NotImplementedException();
        }

        public IPath Path
        {
            get { return new LocalPath("/"); }
        }

        public IDirectory Parent
        {
            get { return null; }
        }

        public IFileSystem FileSystem
        {
            get { return IO.FileSystems.Local; }
        }

        public bool Exists
        {
            get { return true; }
        }

        public string Name
        {
            get { return _entry.Name; }
        }

        public void Delete()
        {
        }

        public string NameWithoutExtension
        {
            get{ return System.IO.Path.GetFileNameWithoutExtension(_entry.Name); }
        }

        public DateTime? LastModifiedTimeUtc
        {
            get { return _entry.DateTime; }
        }

        public Stream Open(FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            return _zip.GetInputStream(_entry);
        }
    }
}