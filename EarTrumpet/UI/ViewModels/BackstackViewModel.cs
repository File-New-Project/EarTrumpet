using EarTrumpet.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace EarTrumpet.UI.ViewModels
{
    class BackstackViewModel : BindableBase
    {
        public bool CanGoBack => _stack.Count > 0;
        public ICommand GoBack { get; }
        public bool IsDisablingUpdates { get; set; }

        private readonly Stack<Action> _stack = new Stack<Action>();

        public BackstackViewModel()
        {
            GoBack = new RelayCommand(() =>
            {
                if (_stack.Count > 0)
                {
                    _stack.Pop().Invoke();
                    RaisePropertyChanged(nameof(CanGoBack));
                }
            });
        }

        public void Add(Action action)
        {
            _stack.Push(action);
            RaisePropertyChanged(nameof(CanGoBack));
        }
    }
}
