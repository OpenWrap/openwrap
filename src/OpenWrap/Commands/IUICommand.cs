namespace OpenWrap.Commands
{
    public interface IUICommand
    {
        UICommandState State { get; }
    }
}