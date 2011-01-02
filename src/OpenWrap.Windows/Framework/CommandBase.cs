using System;
using ICommand = System.Windows.Input.ICommand;

namespace OpenWrap.Windows.Framework
{
    public abstract class CommandBase<T> : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            Execute((T)parameter);
        }

        public virtual bool CanExecute(object parameter)
        {
            return true;
        }

        protected abstract void Execute(T parameter);
    }
}
