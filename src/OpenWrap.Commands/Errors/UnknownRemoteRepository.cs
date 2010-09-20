namespace OpenWrap.Commands.Errors
{
    public class UnknownRemoteRepository : GenericError
    {
        readonly string _remoteName;

        public UnknownRemoteRepository(string remoteName)
        {
            _remoteName = remoteName;
        }

        public override string ToString()
        {
            return "Unknown remote repository '{0}'. To see the list of your remote repositories, use the 'list-remote' command.";
        }
    }
    public class FileNotFound : GenericError
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