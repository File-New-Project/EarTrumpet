using System.Windows;

namespace EarTrumpet.UI.ViewModels
{
    public class ModalDialogViewModel : BindableBase
    {
        private IFocusedViewModel _focused;
        public IFocusedViewModel Focused
        {
            get => _focused;
            set
            {
                if (_focused != value)
                {
                    if (_focused != null)
                    {
                        _focused.Closing();
                    }

                    _focused = value;
                    RaisePropertyChanged(nameof(Focused));
                }
            }
        }

        private FrameworkElement _source;
        public FrameworkElement Source
        {
            get => _source;
            set
            {
                if (_source != value)
                {
                    _source = value;
                    RaisePropertyChanged(nameof(Source));
                }
            }
        }

        private bool _isVisible;
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    RaisePropertyChanged(nameof(IsVisible));

                    if (!_isVisible)
                    {
                        Focused = null;
                        Source = null;
                    }
                }
            }
        }
    }
}
