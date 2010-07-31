namespace OpenWrap.Tasks
{
    public interface ITaskChanges
    {
        void Status(string status);
        void Progress(int progress);
    }
}