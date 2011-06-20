using OpenFileSystem.IO;
using OpenWrap.Runtime;

namespace OpenWrap.ProjectModel
{
    public interface IProject
    {
        TargetFramework TargetFramework { get; }
        string TargetPlatform { get; }
        bool OpenWrapEnabled { get; }
        IFile File { get; }
    }

}