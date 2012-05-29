namespace OpenWrap.Commands.Cli
{
    public interface ICommandOutputFormatter
    {
        void Render(ICommandOutput output);
    }
}