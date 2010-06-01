using System.Collections.Generic;

namespace OpenWrap.Commands.Wrap
{
    [Command(Noun="wrap", Verb="setup")]
    public class SetupWrapCommand : ICommand
    {
        [CommandInput(Name="ProjectName", Position=0, IsRequired=true)]
        public string ProjectName { get; set; }
        public IEnumerable<ICommandResult> Execute()
        {
            yield break;
        }
    }
}