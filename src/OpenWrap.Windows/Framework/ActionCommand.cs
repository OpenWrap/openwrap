using System;
using System.Windows.Input;

namespace OpenWrap.Windows.Framework
{
    public class ActionCommand<T> : ICommand
    {
        private readonly Action<T> _executeAction;

        public ActionCommand(Action<T> executeAction)
        {
            _executeAction = executeAction;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _executeAction((T)parameter);
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }
    }
}
