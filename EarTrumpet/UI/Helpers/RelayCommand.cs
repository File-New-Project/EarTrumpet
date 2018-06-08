using System;
using System.Windows.Input;

namespace EarTrumpet.UI.Helpers
{
    public class RelayCommand : ICommand
    {
        private Action _actionToExecute;

        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action actionToExecute)
        {
            _actionToExecute = actionToExecute;
        }

        public bool CanExecute(object parameter = null)
        {
            return true;
        }

        public void Execute(object parameter = null)
        {
            if (_actionToExecute == null)
            {
                return;
            }

            _actionToExecute.Invoke();
        }

        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged.Invoke(this, null);
            }
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private Action<T> _actionToExecute;

        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action<T> actionToExecute)
        {
            _actionToExecute = actionToExecute;
        }

        public bool CanExecute(object parameter = null)
        {
            return true;
        }

        public void Execute(object parameter = null)
        {
            if (_actionToExecute == null)
            {
                return;
            }

            _actionToExecute.Method.Invoke(_actionToExecute.Target, new object[] { parameter });
        }

        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged.Invoke(this, null);
            }
        }
    }
}
