using System.IO;
using OpenFileSystem.IO.FileSystem.Local;

namespace OpenWrap.Build
{
    public class MSBuildEnvironment : CurrentDirectoryEnvironment
    {
        public MSBuildEnvironment(InitializeOpenWrap initializeOpenWrap, string currentDirectory) : base(Path.GetDirectoryName(initializeOpenWrap.BuildEngine.ProjectFileOfTaskNode))
        {
            if (currentDirectory != null)
                CurrentDirectory = LocalFileSystem.Instance.GetDirectory(currentDirectory);
        }
    }
}