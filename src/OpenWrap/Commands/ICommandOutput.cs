namespace OpenWrap.Commands
{
    public interface ICommandOutput
    {
        ICommand Source { get; }
        CommandResultType Type { get; }
    }
}