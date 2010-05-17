using System;
using System.IO;
using System.Linq;
using OpenRasta.Wrap.Sources;
using OpenWrap.Commands.Wrap;
using IOExtensions=OpenWrap.Commands.Wrap.IOExtensions;

namespace OpenWrap
{
    public class CurrentDirectoryEnvironment : IEnvironment
    {
        public IPackageRepository Repository { get; set; }
        public string WrapDescriptorPath { get; set; }

        public void Initialize()
        {
            WrapDescriptorPath = new DirectoryInfo(Environment.CurrentDirectory)
                .SelfAndAncestors()
                .SelectMany(x => IOExtensions.Files(x, "*.wrapdesc"))
                .FirstOrDefault();
            var dir = new DirectoryInfo(Path.GetDirectoryName(WrapDescriptorPath))
                .SelfAndAncestors()
                .SelectMany(x => x.Directories("wraps"))
                .Where(x => x != null)
                .FirstOrDefault();
            if (dir != null)
            {
                Repository = new FolderRepository(dir.FullName);
            }
        }
    }
}