using OpenWrap.Commands.Wrap;

namespace Tests.Commands.contexts
{
    public class update_wrap : command<UpdateWrapCommand>
    {
        public update_wrap()
        {
            given_remote_config("iron-hills");            
        }
    }
}