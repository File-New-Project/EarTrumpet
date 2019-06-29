using EarTrumpet.UI.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace EarTrumpet.UI.ViewModels
{
    public class FlyoutViewModel : BindableBase, IPopupHostViewModel, IFlyoutViewModel
    {
        public event EventHandler<object> WindowSizeInvalidated;
        public event EventHandler<object> StateChanged;

        public ModalDialogViewModel Dialog { get; }
        public bool IsExpanded { get; private set; }
        public bool IsExpandingOrCollapsing { get; private set; }
        public bool CanExpand => _mainViewModel.AllDevices.Count > 1;
        public string DeviceNameText => Devices.Count > 0 ? Devices[0].DisplayName : null;
        public FlyoutViewState State { get; private set; }
        public ObservableCollection<DeviceViewModel> Devices { get; private set; }
        public ICommand ExpandCollapse { get; private set; }
        public InputType LastInput { get; private set; }
        public ICommand DisplaySettingsChanged { get; }

        private readonly DeviceCollectionViewModel _mainViewModel;
        private readonly DispatcherTimer _deBounceTimer;
        private readonly Dispatcher _currentDispatcher = Dispatcher.CurrentDispatcher;
        private readonly Action _returnFocusToTray;
        private bool _closedDuringOpen;

        public FlyoutViewModel(DeviceCollectionViewModel mainViewModel, Action returnFocusToTray)
        {
            Dialog = new ModalDialogViewModel();
            Devices = new ObservableCollection<DeviceViewModel>();
            _returnFocusToTray = returnFocusToTray;
            _mainViewModel = mainViewModel;
            _mainViewModel.DefaultChanged += OnDefaultPlaybackDeviceChanged;
            _mainViewModel.AllDevices.CollectionChanged += AllDevices_CollectionChanged;
            AllDevices_CollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

            // This timer is used to enable clicking on the tray icon while the flyout is open, and not causing a
            // rapid hide and show cycle.  This time represents the minimum time between which the flyout may be opened.
            _deBounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _deBounceTimer.Tick += OnDeBounceTimerTick;

            ExpandCollapse = new RelayCommand(() =>
            {
                IsExpandingOrCollapsing = true;
                BeginClose(LastInput);
            });
            DisplaySettingsChanged = new RelayCommand(() => BeginClose(InputType.Command));
        }

        private void OnDeBounceTimerTick(object sender, EventArgs e)
        {
            Debug.Assert(State == FlyoutViewState.Closing_Stage2);
            _deBounceTimer.IsEnabled = false;
            ChangeState(FlyoutViewState.Hidden);
        }

        private void AddDevice(DeviceViewModel device)
        {
            if (IsExpanded || Devices.Count == 0)
            {
                device.Apps.CollectionChanged += Apps_CollectionChanged;
                Devices.Insert(0, device);
            }
        }

        private void Apps_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    if (Dialog.Focused is FocusedAppItemViewModel &&
                        (IAppItemViewModel)e.OldItems[0] == ((FocusedAppItemViewModel)Dialog.Focused)?.App)
                    {
                        Dialog.IsVisible = false;
                    }
                    break;
            }

            InvalidateWindowSize();
        }

        private void RemoveDevice(string id)
        {
            var existing = Devices.FirstOrDefault(d => d.Id == id);
            if (existing != null)
            {
                existing.Apps.CollectionChanged -= Apps_CollectionChanged;
                Devices.Remove(existing);
            }
        }

        private void AllDevices_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddDevice((DeviceViewModel)e.NewItems[0]);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    RemoveDevice(((DeviceViewModel)e.OldItems[0]).Id);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    for (int i = Devices.Count - 1; i >= 0; i--)
                    {
                        RemoveDevice(Devices[i].Id);
                    }

                    foreach (var device in _mainViewModel.AllDevices)
                    {
                        AddDevice(device);
                    }

                    OnDefaultPlaybackDeviceChanged(null, _mainViewModel.Default);
                    break;

                default:
                    throw new NotImplementedException();
            }

            if (!CanExpand)
            {
                IsExpanded = false;
            }

            UpdateTextVisibility();
            RaiseDevicesChanged();
        }

        private void RaiseDevicesChanged()
        {
            RaisePropertyChanged(nameof(IsExpanded));
            RaisePropertyChanged(nameof(CanExpand));
            RaisePropertyChanged(nameof(DeviceNameText));
            InvalidateWindowSize();
        }

        private void OnDefaultPlaybackDeviceChanged(object sender, DeviceViewModel e)
        {
            // No longer any devices.
            if (e == null) return;

            var foundDevice = Devices.FirstOrDefault(d => d.Id == e.Id);
            if (foundDevice != null)
            {
                // Push to bottom.
                Devices.Move(Devices.IndexOf(foundDevice), Devices.Count - 1);
            }
            else
            {
                var foundAllDevice = _mainViewModel.AllDevices.FirstOrDefault(d => d.Id == e.Id);
                if (foundAllDevice != null)
                {
                    // We found the device in AllDevices which was not in Devices.
                    // Thus: We are collapsed and can dump the single device in Devices:
                    Devices.Clear();
                    foundAllDevice.Apps.CollectionChanged += Apps_CollectionChanged;
                    Devices.Add(foundAllDevice);
                }
            }
            UpdateTextVisibility();
            RaiseDevicesChanged();
        }

        private void UpdateTextVisibility()
        {
            // Show display name on only the "top" device, which handles Expanded and Collapsed.
            for (var i = 0; i < Devices.Count; i++)
            {
                Devices[i].IsDisplayNameVisible = i > 0;
            }
        }

        public void DoExpandCollapse()
        {
            IsExpanded = !IsExpanded;
            if (IsExpanded)
            {
                // Add any that aren't existing.
                foreach (var device in _mainViewModel.AllDevices)
                {
                    if (!Devices.Contains(device))
                    {
                        device.Apps.CollectionChanged += Apps_CollectionChanged;
                        Devices.Insert(0, device);
                    }
                }
            }
            else
            {
                // Remove all but the default.
                for (int i = Devices.Count - 1; i >= 0; i--)
                {
                    var device = Devices[i];
                    if (device.Id != _mainViewModel.Default?.Id)
                    {
                        device.Apps.CollectionChanged -= Apps_CollectionChanged;
                        Devices.Remove(device);
                    }
                }
            }

            UpdateTextVisibility();
            RaiseDevicesChanged();
        }

        private void InvalidateWindowSize()
        {
            // We must be async because otherwise SetWindowPos will pump messages before the UI has updated.
            _currentDispatcher.BeginInvoke((Action)(() =>
            {
                WindowSizeInvalidated?.Invoke(this, null);
            }));
        }

        public void ChangeState(FlyoutViewState state)
        {
            Trace.WriteLine($"FlyoutViewModel ChangeState {state}");
            ValidateStateChange(state);
            State = state;
            StateChanged?.Invoke(this, null);

            switch (State)
            {
                case FlyoutViewState.Open:
                    _mainViewModel.OnTrayFlyoutShown();

                    if (_closedDuringOpen)
                    {
                        _closedDuringOpen = false;
                        BeginClose(InputType.Command);
                    }
                    break;
                case FlyoutViewState.Closing_Stage1:
                    _mainViewModel.OnTrayFlyoutHidden();
                    Dialog.IsVisible = false;

                    if (LastInput == InputType.Keyboard && !IsExpandingOrCollapsing)
                    {
                        _returnFocusToTray.Invoke();
                    }
                    break;
                case FlyoutViewState.Closing_Stage2:
                    _deBounceTimer.Start();
                    break;
                case FlyoutViewState.Hidden:
                    if (IsExpandingOrCollapsing)
                    {
                        IsExpandingOrCollapsing = false;
                        DoExpandCollapse();
                        BeginOpen(LastInput);
                    }
                    break;
            }
        }

        private void ValidateStateChange(FlyoutViewState newState)
        {
            var oldState = State;
            bool isValidStateTransition =
                (oldState == FlyoutViewState.NotLoaded && newState == FlyoutViewState.Hidden) ||
                (oldState == FlyoutViewState.Hidden && newState == FlyoutViewState.Opening) ||
                (oldState == FlyoutViewState.Opening && newState == FlyoutViewState.Open) ||
                (oldState == FlyoutViewState.Open && newState == FlyoutViewState.Closing_Stage1) ||
                (oldState == FlyoutViewState.Closing_Stage1 && newState == FlyoutViewState.Closing_Stage2) ||
                (oldState == FlyoutViewState.Closing_Stage1 && newState == FlyoutViewState.Hidden) ||
                (oldState == FlyoutViewState.Closing_Stage2 && newState == FlyoutViewState.Hidden);
            Debug.Assert(isValidStateTransition);
        }

        public void OpenPopup(object vm, FrameworkElement container)
        {
            Dialog.IsVisible = false;

            if (vm is IAppItemViewModel)
            {
                Dialog.Focused = new FocusedAppItemViewModel(_mainViewModel, (IAppItemViewModel)vm);
            }
            else if (vm is DeviceViewModel)
            {
                var deviceViewModel = new FocusedDeviceViewModel(_mainViewModel, (DeviceViewModel)vm);
                if (deviceViewModel.IsApplicable)
                {
                    Dialog.Focused = deviceViewModel;
                }
            }
            else
            {
                Dialog.Focused = (IFocusedViewModel)vm;
            }

            if (Dialog.Focused != null)
            {
                Dialog.Focused.RequestClose += () => Dialog.IsVisible = false;
                Dialog.Source = container;
                Dialog.IsVisible = true;
            }
        }

        public void BeginOpen(InputType inputType)
        {
            if (State == FlyoutViewState.Hidden)
            {
                LastInput = inputType;
                ChangeState(FlyoutViewState.Opening);
            }
        }

        public void BeginClose(InputType inputType)
        {
            if (State == FlyoutViewState.Open)
            {
                LastInput = inputType;
                ChangeState(FlyoutViewState.Closing_Stage1);
            }
            else if (State == FlyoutViewState.Opening)
            {
                _closedDuringOpen = true;
            }
        }

        public void OpenFlyout(InputType inputType)
        {
            switch (State)
            {
                case FlyoutViewState.Hidden:
                    BeginOpen(inputType);
                    break;
                case FlyoutViewState.Open:
                    BeginClose(inputType);
                    break;
            }
        }

        public void OnDeactivated(object sender, EventArgs e)
        {
            BeginClose(InputType.Command);
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
                    BeginClose(InputType.Keyboard);
                }
            }
            else if (Keyboard.Modifiers == ModifierKeys.Alt && e.SystemKey == Key.Space)
            {
                // Disable the system menu.
                e.Handled = true;
            }
        }

        public void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            BeginClose(InputType.Keyboard);
        }

        public void OnLightDismissBorderPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Dialog.IsVisible = false;
            e.Handled = true;
        }
    }
}
