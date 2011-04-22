namespace OpenWrap.Commands.Cli.Locators
{
    public interface ICommandLocator
    {
        ICommandDescriptor Execute(ref string input);
    }
}
