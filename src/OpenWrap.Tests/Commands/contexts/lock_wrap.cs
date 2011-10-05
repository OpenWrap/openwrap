using OpenWrap.Commands.Wrap;
using OpenWrap.Repositories;

namespace Tests.Commands.contexts
{
    public abstract class lock_wrap : command<LockWrapCommand>
    {
        protected ISupportLocking project_repo;

        protected override void when_executing_command(string args = null)
        {
            base.when_executing_command(args);
            project_repo = Environment.ProjectRepository as ISupportLocking;
        }
    }
}