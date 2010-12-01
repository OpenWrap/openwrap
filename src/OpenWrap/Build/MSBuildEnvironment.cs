using System.IO;
using OpenFileSystem.IO.FileSystems.Local;

namespace OpenWrap.Build
{
    public class MSBuildEnvironment : CurrentDirectoryEnvironment
    {
        public MSBuildEnvironment(InitializeOpenWrap initializeOpenWrap, string currentDirectory) : base(System.IO.Path.GetDirectoryName(initializeOpenWrap.BuildEngine.ProjectFileOfTaskNode))
        {
            if (currentDirectory != null)
                CurrentDirectory = LocalFileSystem.Instance.GetDirectory(currentDirectory);
        }
    }
}