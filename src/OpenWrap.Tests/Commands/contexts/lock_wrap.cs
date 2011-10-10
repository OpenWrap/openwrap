using System;
using System.Collections.Generic;
using System.Linq;
using OpenWrap;
using OpenWrap.Commands;
using OpenWrap.Commands.Wrap;
using OpenWrap.Repositories;

namespace Tests.Commands.contexts
{
    public abstract class locking_command<T> : command<T> where T : ICommand
    {
        protected ISupportLocking project_repo;

        protected override void when_executing_command(string args = null)
        {
            base.when_executing_command(args);

            project_repo = Environment.ProjectRepository == null? null : Environment.ProjectRepository.Feature<ISupportLocking>();
            
        }
    }
    public abstract class lock_wrap : locking_command<LockWrapCommand>
    {
    }
    public abstract class unlock_wrap : locking_command<UnlockWrapCommand>
    {
    }
}