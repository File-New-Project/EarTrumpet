using EarTrumpet.UI.Helpers;
using EarTrumpet.Interop.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using EarTrumpet.DataModel.WindowsAudio;

namespace EarTrumpet.UI.ViewModels
{
    public class FlyoutViewModel : BindableBase, IPopupHostViewModel, IFlyoutViewModel
    {
        public event EventHandler<object> WindowSizeInvalidated;
        public event EventHandler<object> StateChanged;

        public ModalDialogViewModel Dialog { get; }
        public bool IsExpanded { get; private set; }
        public bool IsExpandingOrCollapsing { get; private set; }
        public bool CanExpand => _currentDevicesViewModel.AllDevices.Count > 1;
        public string DeviceNameText => CurrentDevices.Count > 0 ? CurrentDevices[0].DisplayName : null;
        public FlyoutViewState State { get; private set; }
        public ObservableCollection<DeviceViewModel> CurrentDevices { get; private set; }
        public ICommand ExpandCollapse { get; private set; }
        public InputType LastInput { get; private set; }
        public ICommand DisplaySettingsChanged { get; }
        public AudioDeviceKind ShownDeviceType { get; private set; }
        public bool ShowDeviceTypeSwitchInFlyout { get; private set; }
        public ICommand SwitchShownDeviceType { get; private set; }

        private DeviceCollectionViewModel _currentDevicesViewModel;
        private readonly DeviceCollectionViewModel _playbackViewModel;
        private readonly DeviceCollectionViewModel _recordingViewModel;
        private readonly ObservableCollection<DeviceViewModel> _playbackDevices;
        private readonly ObservableCollection<DeviceViewModel> _recordingDevices;
        private readonly DispatcherTimer _deBounceTimer;
        private readonly Dispatcher _currentDispatcher = Dispatcher.CurrentDispatcher;
        private readonly Action _returnFocusToTray;
        private readonly AppSettings _settings;
        private bool _closedDuringOpen;
        private MouseHook _mh;
        private Rect _winRect;

        public FlyoutViewModel(DeviceCollectionViewModel playbackViewModel, DeviceCollectionViewModel recordingViewModel, Action returnFocusToTray, AppSettings settings)
        {
            _settings = settings;
            IsExpanded = _settings.IsExpanded;
            ShowDeviceTypeSwitchInFlyout = _settings.ShowDeviceTypeSwitchInFlyout;
            _settings.ShowDeviceTypeSwitchInFlyoutChanged += OnShowDeviceTypeSwitchInFlyoutChanged;
            Dialog = new ModalDialogViewModel();
            _returnFocusToTray = returnFocusToTray;

            _playbackDevices = new ObservableCollection<DeviceViewModel>();
            _playbackViewModel = playbackViewModel;
            _playbackViewModel.DefaultChanged += OnDefaultPlaybackDeviceChanged;
            _playbackViewModel.AllDevices.CollectionChanged += PlaybackDevices_CollectionChanged;
            PlaybackDevices_CollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

            _recordingDevices = new ObservableCollection<DeviceViewModel>();
            _recordingViewModel = recordingViewModel;
            _recordingViewModel.DefaultChanged += OnDefaultRecordingDeviceChanged;
            _recordingViewModel.AllDevices.CollectionChanged += RecordingDevices_CollectionChanged;
            RecordingDevices_CollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

            DoChangeShownDeviceType(AudioDeviceKind.Playback);

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
            SwitchShownDeviceType = new RelayCommand(DoSwitchShownDeviceType);

            _mh = new MouseHook();
            _mh.MouseWheelEvent += OnMouseWheelEvent;
        }

        private void OnShowDeviceTypeSwitchInFlyoutChanged(object sender, bool showDeviceTypeSwitchInFlyout)
        {
            ShowDeviceTypeSwitchInFlyout = showDeviceTypeSwitchInFlyout;
            RaisePropertyChanged(nameof(ShowDeviceTypeSwitchInFlyout));

            if (showDeviceTypeSwitchInFlyout == false && CurrentDevices == _recordingDevices)
            {
                DoChangeShownDeviceType(AudioDeviceKind.Playback);
            }
            else
            {
                UpdateTextVisibility(CurrentDevices);
                InvalidateWindowSize();
            }
        }

        public void UpdateWindowPos(double top, double left, double height, double width)
        {
            _winRect = new Rect(left, top, width, height);
        }

        private void OnDeBounceTimerTick(object sender, EventArgs e)
        {
            Debug.Assert(State == FlyoutViewState.Closing_Stage2);
            _deBounceTimer.IsEnabled = false;
            ChangeState(FlyoutViewState.Hidden);
        }

        private void AddDevice(ObservableCollection<DeviceViewModel> devices, DeviceViewModel device)
        {
            if (IsExpanded || devices.Count == 0)
            {
                device.Apps.CollectionChanged += Apps_CollectionChanged;
                devices.Insert(0, device);
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

        private void RemoveDevice(ObservableCollection<DeviceViewModel> devices, string id)
        {
            var existing = devices.FirstOrDefault(d => d.Id == id);
            if (existing != null)
            {
                existing.Apps.CollectionChanged -= Apps_CollectionChanged;
                devices.Remove(existing);
            }
        }

        private void PlaybackDevices_CollectionChanged(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Devices_CollectionChanged(sender, _playbackDevices, _playbackViewModel, e);
        }

        private void RecordingDevices_CollectionChanged(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Devices_CollectionChanged(sender, _recordingDevices, _recordingViewModel, e);
        }

        private void Devices_CollectionChanged(object sender, ObservableCollection<DeviceViewModel> devices, DeviceCollectionViewModel collectionViewModel, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddDevice(devices, (DeviceViewModel)e.NewItems[0]);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    RemoveDevice(devices, ((DeviceViewModel)e.OldItems[0]).Id);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    for (int i = devices.Count - 1; i >= 0; i--)
                    {
                        RemoveDevice(devices, devices[i].Id);
                    }

                    foreach (var device in collectionViewModel.AllDevices)
                    {
                        AddDevice(devices, device);
                    }

                    OnDefaultDeviceChanged(null, devices, collectionViewModel, collectionViewModel.Default);
                    break;

                default:
                    throw new NotImplementedException();
            }

            UpdateTextVisibility(devices);
            RaiseDevicesChanged();
        }

        private void RaiseDevicesChanged()
        {
            RaisePropertyChanged(nameof(IsExpanded));
            RaisePropertyChanged(nameof(CanExpand));
            RaisePropertyChanged(nameof(DeviceNameText));
            RaisePropertyChanged(nameof(CurrentDevices));
            RaisePropertyChanged(nameof(ShownDeviceType));
            InvalidateWindowSize();
        }

        private void OnDefaultPlaybackDeviceChanged(object sender,
             DeviceViewModel e)
        {
            OnDefaultDeviceChanged(sender, _playbackDevices, _playbackViewModel, e);
        }

        private void OnDefaultRecordingDeviceChanged(object sender,
             DeviceViewModel e)
        {
            OnDefaultDeviceChanged(sender, _recordingDevices, _recordingViewModel, e);
        }

        private void OnDefaultDeviceChanged(object sender, ObservableCollection<DeviceViewModel> devices, DeviceCollectionViewModel collectionViewModel, DeviceViewModel e)
        {
            // No longer any devices.
            if (e == null) return;

            var foundDevice = devices.FirstOrDefault(d => d.Id == e.Id);
            if (foundDevice != null)
            {
                // Push to bottom.
                devices.Move(devices.IndexOf(foundDevice), devices.Count - 1);
            }
            else
            {
                var foundAllDevice = collectionViewModel.AllDevices.FirstOrDefault(d => d.Id == e.Id);
                if (foundAllDevice != null)
                {
                    // We found the device in `AllDevices` which was not in `devices`.
                    // Thus: We are collapsed and can dump the single device in `devices`:
                    devices.Clear();
                    foundAllDevice.Apps.CollectionChanged += Apps_CollectionChanged;
                    devices.Add(foundAllDevice);
                }
            }
            UpdateTextVisibility(devices);
            RaiseDevicesChanged();
        }

        private void UpdateTextVisibility(ObservableCollection<DeviceViewModel> devices)
        {
            // Show display name on only the "top" device, which handles Expanded and Collapsed.
            for (var i = 0; i < devices.Count; i++)
            {
                devices[i].IsDisplayNameVisible = i > 0;
                devices[i].IsShouldShowDoubleTitleCellHeight = ShowDeviceTypeSwitchInFlyout && i == 0;
            }
        }

        public void DoExpandCollapse()
        {
            IsExpanded = !IsExpanded;
            _settings.IsExpanded = IsExpanded;

            UpdateCurrentDevicesList();
        }

        /// <summary>
        /// Ensures that `CurrentDevices` has correct items according to `IsExpanded` value
        /// </summary>
        private void UpdateCurrentDevicesList()
        {
            if (IsExpanded)
            {
                // Add any that aren't existing.
                foreach (var device in _currentDevicesViewModel.AllDevices)
                {
                    if (!CurrentDevices.Contains(device))
                    {
                        device.Apps.CollectionChanged += Apps_CollectionChanged;
                        CurrentDevices.Insert(0, device);
                    }
                }
            }
            else
            {
                // Remove all but the default.
                for (int i = CurrentDevices.Count - 1; i >= 0; i--)
                {
                    var device = CurrentDevices[i];
                    if (device.Id != _currentDevicesViewModel.Default?.Id)
                    {
                        device.Apps.CollectionChanged -= Apps_CollectionChanged;
                        CurrentDevices.Remove(device);
                    }
                }
            }

            UpdateTextVisibility(CurrentDevices);
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

        private void DoChangeShownDeviceType(AudioDeviceKind kind)
        {
            ShownDeviceType = kind;
            CurrentDevices = kind == AudioDeviceKind.Playback ? _playbackDevices : _recordingDevices;
            _currentDevicesViewModel = kind == AudioDeviceKind.Playback ? _playbackViewModel : _recordingViewModel;

            UpdateCurrentDevicesList();
        }

        private void DoSwitchShownDeviceType()
        {
            DoChangeShownDeviceType(ShownDeviceType == AudioDeviceKind.Playback
                ? AudioDeviceKind.Recording
                : AudioDeviceKind.Playback);
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
                    _currentDevicesViewModel.OnTrayFlyoutShown();

                    if (_closedDuringOpen)
                    {
                        _closedDuringOpen = false;
                        BeginClose(InputType.Command);
                    }
                    break;
                case FlyoutViewState.Closing_Stage1:
                    _currentDevicesViewModel.OnTrayFlyoutHidden();
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
                Dialog.Focused = new FocusedAppItemViewModel(_currentDevicesViewModel, (IAppItemViewModel)vm);
            }
            else if (vm is DeviceViewModel)
            {
                var deviceViewModel = new FocusedDeviceViewModel(_currentDevicesViewModel, (DeviceViewModel)vm);
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

        private int OnMouseWheelEvent(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            var existing = _currentDevicesViewModel.Default;
            if (existing != null)
            {
                if (!_winRect.Contains(new Point(e.X, e.Y)))
                {
                    existing.IncrementVolume(Math.Sign(e.Delta) * 2);
                    return -1;
                }
            }
            return 0;
        }

        public void BeginOpen(InputType inputType)
        {
            if (State == FlyoutViewState.Hidden)
            {
                LastInput = inputType;
                ChangeState(FlyoutViewState.Opening);
            }

            if (_settings.UseGlobalMouseWheelHook)
            {
                _mh.SetHook();
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

            _mh.UnHook();
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
            if (State == FlyoutViewState.Opening)
            {
                return;
            }
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
