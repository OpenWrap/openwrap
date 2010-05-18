using OpenWrap.Build.Services;
using OpenWrap.Commands;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;

namespace OpenWrap.Commands.Wrap
{
    [Command(Verb = "sync", Namespace = "wrap")]
    public class SyncWrapCommand : ICommand
    {
        public ICommandResult Execute()
        {
            var packageManager = WrapServices.GetService<IPackageManager>();
            var environment = WrapServices.GetService<IEnvironment>();

            var descriptor = new WrapDescriptorParser().ParseFile(environment.DescriptorPath);
            var dependencyResolveResult = packageManager.TryResolveDependencies(descriptor, environment.ProjectRepository, environment.UserRepository, environment.RemoteRepositories);
            return new Success();
        }
    }
}