using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace EarTrumpet.UI.ViewModels
{
    public class FullWindowViewModel : BindableBase
    {
        public event EventHandler<AppExpandedEventArgs> AppExpanded = delegate { };
        public event EventHandler<object> AppCollapsed = delegate { };

        public ObservableCollection<DeviceViewModel> AllDevices => _mainViewModel.AllDevices;
        public bool IsShowingModalDialog { get; private set; }

        DeviceCollectionViewModel _mainViewModel;

        public FullWindowViewModel(DeviceCollectionViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

            _mainViewModel.OnFullWindowOpened();
        }

        public void Close()
        {
            CollapseApp();
            _mainViewModel.OnFullWindowClosed();
        }

        public void ExpandApp(IAppItemViewModel vm, UIElement container)
        {
            if (IsShowingModalDialog)
            {
                CollapseApp();
            }

            AppExpanded?.Invoke(this, new AppExpandedEventArgs { Container = container, ViewModel = vm });

            IsShowingModalDialog = true;
            RaisePropertyChanged(nameof(IsShowingModalDialog));
        }

        public void CollapseApp()
        {
            if (IsShowingModalDialog)
            {
                AppCollapsed?.Invoke(this, null);
                IsShowingModalDialog = false;
                RaisePropertyChanged(nameof(IsShowingModalDialog));
            }
        }
    }
}
