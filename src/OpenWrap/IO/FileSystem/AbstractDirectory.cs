namespace OpenWrap.IO.FileSystem
{
    public abstract class AbstractDirectory
    {
        protected string NormalizeDirectoryPath(string directoryPath)
        {
            if (!directoryPath.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString())
                && !directoryPath.EndsWith(System.IO.Path.AltDirectorySeparatorChar.ToString()))
                return directoryPath + System.IO.Path.DirectorySeparatorChar;
            return directoryPath;
        }
    }
}