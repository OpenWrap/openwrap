using System;
using OpenRasta.Client;
using OpenWrap.Commands;

namespace OpenWrap.Tasks
{
    internal class TaskProgressMessage : ICommandOutput, IProgressOutput
    {
        readonly ITask _task;

        public TaskProgressMessage(ITask task)
        {
            _task = task;
            _task.ProgressChanged += (s, e) => this.ProgressChanged.Raise(this, e);
            _task.StatusChanged += (s, e) => StatusChanged.Raise(this, e);
        }

        public event EventHandler Complete;

        public event EventHandler<ProgressEventArgs> ProgressChanged;
        public event EventHandler<StatusChangedEventArgs> StatusChanged;

        public ICommand Source
        {
            get { throw new NotImplementedException(); }
        }

        public bool Success
        {
            get { return true; }
        }

        public CommandResultType Type
        {
            get { return CommandResultType.Info; }
        }
    }
}