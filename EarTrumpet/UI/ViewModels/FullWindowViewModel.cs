using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace EarTrumpet.UI.ViewModels
{
    public class FullWindowViewModel : BindableBase
    {
        public ObservableCollection<DeviceViewModel> AllDevices => _mainViewModel.AllDevices;
        public bool IsShowingModalDialog
        {
            get => _isShowingModalDialog;
            set
            {
                if (_isShowingModalDialog != value)
                {
                    _isShowingModalDialog = value;
                    RaisePropertyChanged(nameof(IsShowingModalDialog));

                    if (!_isShowingModalDialog)
                    {
                        Focused = null;
                        FocusedSource = null;

                        RaisePropertyChanged(nameof(Focused));
                        RaisePropertyChanged(nameof(FocusedSource));
                    }
                }
            }
        }
        public FocusedAppItemViewModel Focused { get; private set; }
        public UIElement FocusedSource { get; private set; }

        private DeviceCollectionViewModel _mainViewModel;
        private bool _isShowingModalDialog;

        public FullWindowViewModel(DeviceCollectionViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            _mainViewModel.AppPopup += OnAppPopup;
            _mainViewModel.OnFullWindowOpened();
        }

        public void Close()
        {
            IsShowingModalDialog = false;
            _mainViewModel.OnFullWindowClosed();
        }

        public void OnAppPopup(IAppItemViewModel vm, UIElement container)
        {
            if (Window.GetWindow(container).DataContext != this)
            {
                return;
            }

            IsShowingModalDialog = false;

            Focused = new FocusedAppItemViewModel(_mainViewModel, vm);
            Focused.RequestClose += () => IsShowingModalDialog = false;
            FocusedSource = container;
            RaisePropertyChanged(nameof(Focused));
            RaisePropertyChanged(nameof(FocusedSource));

            IsShowingModalDialog = true;
        }
    }
}
