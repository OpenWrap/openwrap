using System;
using System.Collections.Generic;
using System.Linq;
using SysPath = System.IO.Path;

namespace OpenWrap.IO
{
    public class InMemoryFileSystem : IFileSystem
    {
        public InMemoryFileSystem()
        {
            
        }

        public InMemoryFileSystem(params IDirectory[] childDirectories)
        {
            //ChildDirectories.AddRange((from directory in childDirectories
            //                           from localDirectory in directory.AncestorsAndSelf()
            //                           orderby localDirectory.Path.FullPath ascending
            //                           select localDirectory).Distinct().ToLookup);

        }

        public List<IDirectory> ChildDirectories { get; set; }

        public IDirectory GetDirectory(string directoryPath)
        {
            var resolvedDirectoryPath = SysPath.GetFullPath(SysPath.Combine(CurrentDirectory,directoryPath));

            return new InMemoryDirectory(directoryPath) 
            {
                FileSystem = this,
                Exists = ChildDirectories.Any(x=>x.Path.FullPath.Equals(resolvedDirectoryPath))
            };
        }

        public IPath GetPath(string path)
        {
            return new LocalPath(path);
        }

        public ITemporaryDirectory CreateTempDirectory()
        {
            return new InMemoryTemporaryDirectory(SysPath.Combine("c:\temporary", SysPath.GetRandomFileName()))
            {
                Exists = true,
                FileSystem = this
            };
        }

        public IDirectory CreateDirectory(string path)
        {
            return new InMemoryDirectory(path)
            {
                FileSystem = this,
                Exists = true
            };
        }

        public IFile GetFile(string itemSpec)
        {
            return new InMemoryFile(itemSpec)
            {
                FileSystem = this
            };
        }

        public ITemporaryFile CreateTempFile()
        {
            return new InMemoryTemporaryFile(SysPath.Combine("c:\temporary", SysPath.GetRandomFileName()))
            {
                Exists = true,
                FileSystem = this
            };
        }

        public IDirectory GetTempDirectory()
        {
            return new InMemoryTemporaryDirectory(SysPath.GetTempPath())
            {
                Exists = true,
                FileSystem = this
            
            
            };
        }

        public string CurrentDirectory { get; set; }
    }
}