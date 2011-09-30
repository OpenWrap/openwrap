using OpenWrap.Commands.Cli.Locators;

namespace Tests.Commands.contexts
{
    abstract class noun_verb_locator : command_locator<NounVerbCommandLocator>
    {
        public noun_verb_locator() : base(_=>new NounVerbCommandLocator(_))
        {   
        }
    }
}