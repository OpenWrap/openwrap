using System;
using System.Collections.Generic;
using OpenWrap.Commands;
using ICommand = System.Windows.Input.ICommand;

namespace OpenWrap.Windows.Framework
{
    public abstract class CommandBase<T> : ICommand
    {
        protected IEnumerable<ICommandOutput> _commandOutput;

        protected abstract void Execute(T parameter);

        public void Execute(object parameter)
        {
            Execute((T)parameter);
        }

        public virtual bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public IEnumerable<ICommandOutput> CommandOutput
        {
            get { return _commandOutput; }
        }
    }
}
