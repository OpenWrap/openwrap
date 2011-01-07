using System;
using System.Windows.Input;

namespace OpenWrap.Windows.Framework
{
    /// <summary>
    /// Non-generic command base class. For when the execute parameter is not used
    /// </summary>
    public abstract class CommandBase : ICommand
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


        public abstract void Execute(object parameter);

        public virtual bool CanExecute(object parameter)
        {
            return true;
        }
    }

    /// <summary>
    /// Generic command base class, with a strongly-typed parameter
    /// </summary>
    /// <typeparam name="T"></typeparam>
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
