using System.IO;
using OpenFileSystem.IO.FileSystems.Local;
using OpenWrap.Runtime;

namespace OpenWrap.Build
{
    public class MSBuildEnvironment : CurrentDirectoryEnvironment
    {
        public MSBuildEnvironment(string projectFileDirectory, string currentDirectory) 
            : base(projectFileDirectory)
        {
            if (currentDirectory != null)
                CurrentDirectory = LocalFileSystem.Instance.GetDirectory(currentDirectory);
        }
    }
}