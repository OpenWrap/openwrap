using System;
using System.Windows.Input;
using ICommand = System.Windows.Input.ICommand;

namespace OpenWrap.Windows.Framework
{
    public abstract class CommandBase<T> : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }

            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public void Execute(object parameter)
        {
            Execute((T)parameter);
        }

        public virtual bool CanExecute(object parameter)
        {
            return CanExecute((T)parameter);
        }
        
        protected abstract void Execute(T parameter);

        protected virtual bool CanExecute(T parameter)
        {
            return true;
        }
    }
}
