namespace OpenWrap.Commands
{
    public interface IUICommandDescriptor : ICommandDescriptor
    {
        string Label { get; }
        UICommandContext Context { get; }
    }
}