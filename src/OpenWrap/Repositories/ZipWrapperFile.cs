﻿using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.Local;
using Path = OpenFileSystem.IO.Path;

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

        public Path Path
        {
            get { return new Path("/"); }
        }

        public IDirectory Parent
        {
            get { return null; }
        }

        public IFileSystem FileSystem
        {
            get { return LocalFileSystem.Instance; }
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

        public string Extension
        {
            get { return System.IO.Path.GetExtension(_entry.Name); }
        }

        public long Size
        {
            get {return _entry.Size; }
        }

        public DateTime? LastModifiedTimeUtc
        {
            get { return _entry.DateTime; }
        }

        public Stream Open(FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            return _zip.GetInputStream(_entry);
        }

        #region IFileSystemItem Members

        public void CopyTo(IFileSystemItem item)
        {
            
        }

        public void MoveTo(IFileSystemItem item)
        {
            
        }

        #endregion
    }
}