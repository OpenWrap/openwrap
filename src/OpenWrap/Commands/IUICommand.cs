namespace OpenWrap.Commands
{
    public interface IUICommand
    {
        UICommandState State { get; }
    }

    public enum UICommandState
    {
        Enabled,
        Disabled,
        Hidden
    }
}