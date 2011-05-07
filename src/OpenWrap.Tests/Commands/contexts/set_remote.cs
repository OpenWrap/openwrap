using OpenWrap.Commands.contexts;
using OpenWrap.Commands.Remote;
using OpenWrap.Configuration;
using Tests.Commands.contexts;

namespace OpenWrap.Tests.Commands.Remote.Set.context
{
    public class set_remote : remote_command<SetRemoteCommand>
    {
        public set_remote()
        {
            given_remote_configuration(
                    new RemoteRepositories
                    {
                            { "primus", new RemoteRepository { Name = "primus", Priority = 1, FetchRepository = "[memory]primus"} },
                            { "secundus", new RemoteRepository { Name = "secundus", Priority = 2, FetchRepository = "[memory]secundus" } },
                            { "terz", new RemoteRepository { Name = "terz", Priority = 3, FetchRepository = "[memory]terz"} }
                    });
        }

        public RemoteRepository TryGetRepository(string name)
        {
            var repositories = Services.ServiceLocator.GetService<IConfigurationManager>().LoadRemoteRepositories();
            RemoteRepository rep;
            repositories.TryGetValue(name, out rep);
            return rep;
        }
    }
}