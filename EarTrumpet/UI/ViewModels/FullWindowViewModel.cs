using EarTrumpet.UI.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace EarTrumpet.UI.ViewModels
{
    public class FullWindowViewModel : BindableBase, IPopupHostViewModel
    {
        public static readonly int SmallDeviceCountLimit = 3;

        public ObservableCollection<DeviceViewModel> AllDevices => _mainViewModel.AllDevices;
        public ModalDialogViewModel Dialog { get; }
        public ICommand DisplaySettingsChanged { get; }
        public bool IsManyDevicesMode => AllDevices.Count > SmallDeviceCountLimit;

        private readonly DeviceCollectionViewModel _mainViewModel;
        private WindowViewState _state;

        public FullWindowViewModel(DeviceCollectionViewModel mainViewModel)
        {
            Dialog = new ModalDialogViewModel();
            _mainViewModel = mainViewModel;
            _mainViewModel.OnFullWindowOpened();
            _mainViewModel.AllDevices.CollectionChanged += OnDevicesChanged;

            DisplaySettingsChanged = new RelayCommand(() => Dialog.IsVisible = false);
        }

        private void OnDevicesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(IsManyDevicesMode));
        }

        public void OpenPopup(object vm, FrameworkElement container)
        {
            Dialog.IsVisible = false;

            if (vm is IAppItemViewModel)
            {
                Dialog.Focused = new FocusedAppItemViewModel(_mainViewModel, (IAppItemViewModel)vm);
            }
            else
            {
                var deviceViewModel = new FocusedDeviceViewModel(_mainViewModel, (DeviceViewModel)vm);
                if (deviceViewModel.IsApplicable)
                {
                    Dialog.Focused = deviceViewModel;
                }
            }

            if (Dialog.Focused != null)
            {
                Dialog.Focused.RequestClose += () => Dialog.IsVisible = false;
                Dialog.Source = container;
                Dialog.IsVisible = true;
            }
        }

        public void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            switch (_state)
            {
                case WindowViewState.Open:
                    _state = WindowViewState.Closing;
                    e.Cancel = true;

                    Dialog.IsVisible = false;
                    _mainViewModel.OnFullWindowClosed();

                    var window = (Window)sender;
                    WindowAnimationLibrary.BeginWindowExitAnimation(window, () =>
                    {
                        _state = WindowViewState.CloseReady;
                        window.Close();
                    });
                    break;
                case WindowViewState.Closing:
                    // Ignore any requests while playing the close animation.
                    e.Cancel = true;
                    break;
                case WindowViewState.CloseReady:
                    // Accept the close.
                    break;
            }
        }

        public void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (Dialog.IsVisible)
                {
                    Dialog.IsVisible = false;
                }
                else
                {
                    ((Window)sender).Close();
                }
            }
        }

        public void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Dialog.IsVisible = false;
        }

        public void OnLocationChanged(object sender, EventArgs e)
        {
            Dialog.IsVisible = false;
        }

        public void OnLightDismissBorderPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Dialog.IsVisible = false;
            e.Handled = true;
        }
    }
}
