﻿namespace OpenWrap.Preloading.TinySharpZip
{
    public class ZipDirectoryEntry : ZipEntry
    {
        public ZipDirectoryEntry(string directoryName)
        {
            this.DirectoryName = directoryName;
        }

        public string DirectoryName { get; set; }
    }
}