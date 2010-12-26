using System.IO;
using OpenFileSystem.IO.FileSystems.Local;
using OpenWrap.Runtime;

namespace OpenWrap.Build
{
    public class MSBuildEnvironment : CurrentDirectoryEnvironment
    {
        public MSBuildEnvironment(InitializeOpenWrap initializeOpenWrap, string currentDirectory) 
            : base(Path.GetDirectoryName(initializeOpenWrap.BuildEngine.ProjectFileOfTaskNode))
        {
            if (currentDirectory != null)
                CurrentDirectory = LocalFileSystem.Instance.GetDirectory(currentDirectory);
        }
    }
}