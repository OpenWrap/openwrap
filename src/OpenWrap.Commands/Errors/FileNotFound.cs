namespace OpenWrap.Commands.Errors
{
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