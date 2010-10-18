namespace OpenWrap.Commands.Errors
{
    public class UnknownRemoteRepository : Error
    {
        readonly string _remoteName;

        public UnknownRemoteRepository(string remoteName)
        {
            _remoteName = remoteName;
        }

        public override string ToString()
        {
            return string.Format("Unknown remote repository '{0}'. To see the list of your remote repositories, use the 'list-remote' command.", _remoteName);
        }
    }
    public class FileNotFound : Error
    {
        readonly string _filePath;

        public FileNotFound(string filePath)
        {
            _filePath = filePath;
        }
        public override string ToString()
        {
            return string.Format("File '{0}' was not found. Check the file name and try again.", _filePath);
        }
    }
}