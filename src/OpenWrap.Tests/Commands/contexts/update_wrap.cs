using OpenWrap.Commands.Wrap;

namespace Tests.Commands.contexts
{
    public abstract class update_wrap : command<UpdateWrapCommand>
    {
        public update_wrap()
        {
            given_remote_repository("iron-hills");
        }
    }
}