using System;
using System.Windows.Input;

namespace OpenWrap.Windows.Framework
{
    /// <summary>
    /// AKA "RelayCommand" 
    /// Used to wrap up actions or lambdas as a WPF command
    /// </summary>
    /// <typeparam name="T">The command param type</typeparam>
    public class ActionCommand<T> : ICommand
    {
        private readonly Action<T> _executeAction;
        private readonly Predicate<T> _canExecuteTest;

        public ActionCommand(Action<T> executeAction)
        {
            _executeAction = executeAction;
            _canExecuteTest = null;
        }

        public ActionCommand(Action<T> executeAction, Predicate<T> canExecuteTest)
        {
            _executeAction = executeAction;
            _canExecuteTest = canExecuteTest;
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canExecuteTest != null)
                {
                    CommandManager.RequerySuggested += value;
                }
            }

            remove
            {
                if (_canExecuteTest != null)
                {
                    CommandManager.RequerySuggested -= value;
                }
            }
        }

        public void Execute(object parameter)
        {
            _executeAction((T)parameter);
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecuteTest != null)
            {
                return _canExecuteTest((T)parameter);
            }

            return true;
        }
    }
}
