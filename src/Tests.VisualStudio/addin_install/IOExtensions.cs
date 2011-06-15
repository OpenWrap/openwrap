using OpenFileSystem.IO;

namespace Tests.VisualStudio.addin_install
{
    public static class IOExtensions
    {
        public static string ToFileUri(this Path path)
        {
            return "file:///" + path.FullPath.Replace("\\", "/");
        }
    }
}