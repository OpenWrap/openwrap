using System.Linq;
using OpenWrap.Commands.Remote;
using OpenWrap.Configuration.Remotes;

namespace Tests.Commands.contexts
{
    public abstract class set_remote : remote_command<SetRemoteCommand>
    {

        protected set_remote()
        {
        }

        public RemoteRepository TryGetRepository(string name)
        {
            RemoteRepository rep;
            ConfiguredRemotes.TryGetValue(name, out rep);
            return rep;
        }
    }
}