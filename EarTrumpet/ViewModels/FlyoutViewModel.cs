using EarTrumpet.DataModel;
using EarTrumpet.Misc;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace EarTrumpet.ViewModels
{
    public class FlyoutViewModel : BindableBase
    {
        public enum ViewState
        {
            NotReady,
            Hidden,
            Opening,
            Open,
            Closing_Stage1,
            Closing_Stage2, // Delay stage
        }

        public enum CloseReason
        {
            Normal,
            CloseThenOpen,
        }

        public event EventHandler<AppExpandedEventArgs> AppExpanded = delegate { };
        public event EventHandler<object> WindowSizeInvalidated = delegate { };
        public event EventHandler<object> AppCollapsed = delegate { };
        public event EventHandler<CloseReason> StateChanged = delegate { };

        public bool IsExpanded { get; private set; }
        public bool CanExpand => _mainViewModel.AllDevices.Count > 1;
        public bool IsEmpty => Devices.Count == 0;
        public string ExpandText => CanExpand ? (IsExpanded ? "\ue011" : "\ue010") : "";
        public string ExpandAccessibleText => CanExpand ? (IsExpanded ? Properties.Resources.CollapseAccessibleText : Properties.Resources.ExpandAccessibleText) : "";
        public ViewState State { get; private set; }
        public bool IsShowingModalDialog { get; private set; }
        public ObservableCollection<DeviceViewModel> Devices { get; private set; }
        public RelayCommand ExpandCollapse { get; private set; }

        private readonly IAudioDeviceManager _deviceManager;
        private readonly MainViewModel _mainViewModel;
        private readonly DispatcherTimer _hideTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
        private bool _closedOnOpen;
        private bool _expandOnCloseThenOpen;

        internal FlyoutViewModel(MainViewModel mainViewModel, IAudioDeviceManager deviceManager)
        {
            Devices = new ObservableCollection<DeviceViewModel>();

            _deviceManager = deviceManager;
            _mainViewModel = mainViewModel;
            _mainViewModel.FlyoutShowRequested += (_, __) => OpenFlyout();

            _deviceManager.DefaultPlaybackDeviceChanged += OnDefaultPlaybackDeviceChanged;

            _mainViewModel.AllDevices.CollectionChanged += AllDevices_CollectionChanged;

            AllDevices_CollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

            _hideTimer.Tick += HideTimer_Tick;

            ExpandCollapse = new RelayCommand(() => BeginClose(FlyoutViewModel.CloseReason.CloseThenOpen));
        }

        private void HideTimer_Tick(object sender, EventArgs e)
        {
            Debug.Assert(State == ViewState.Closing_Stage2);
            _hideTimer.IsEnabled = false;
            ChangeState(ViewState.Hidden);
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

                    OnDefaultPlaybackDeviceChanged(null, _deviceManager.DefaultPlaybackDevice);
                    break;

                default:
                    throw new NotImplementedException();
            }

            if (!CanExpand)
            {
                IsExpanded = false;
            }

            RaisePropertyChanged(nameof(IsEmpty));
            RaisePropertyChanged(nameof(CanExpand));
            RaisePropertyChanged(nameof(ExpandText));
            RaisePropertyChanged(nameof(ExpandAccessibleText));
            InvalidateWindowSize();
        }

        private void OnDefaultPlaybackDeviceChanged(object sender, IAudioDevice e)
        {
            // no longer any device
            if (e == null) return;

            var foundDevice = Devices.FirstOrDefault(d => d.Id == e.Id);
            if (foundDevice != null)
            {
                // Move to bottom.
                Devices.Move(Devices.IndexOf(foundDevice), Devices.Count - 1);
            }
            else
            {
                var foundAllDevice = _mainViewModel.AllDevices.FirstOrDefault(d => d.Id == e.Id);
                if (foundAllDevice != null)
                {
                    Devices.Clear();
                    foundAllDevice.Apps.CollectionChanged += Apps_CollectionChanged;
                    Devices.Add(foundAllDevice);
                    InvalidateWindowSize();
                }
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
                // Remove all but default.
                for (int i = Devices.Count - 1; i >= 0; i--)
                {
                    var device = Devices[i];

                    if (device.Id != _deviceManager.DefaultPlaybackDevice.Id)
                    {
                        device.Apps.CollectionChanged -= Apps_CollectionChanged;
                        Devices.Remove(device);
                    }
                }
            }

            RaisePropertyChanged(nameof(IsExpanded));
            RaisePropertyChanged(nameof(ExpandText));
            RaisePropertyChanged(nameof(ExpandAccessibleText));
            InvalidateWindowSize();
        }

        private void InvalidateWindowSize()
        {
            // We must be async because otherwise SetWindowPos will pump messages before the UI has updated.
            App.Current.Dispatcher.BeginInvoke((Action)(() => {
                WindowSizeInvalidated?.Invoke(this, null);
            }));
        }

        public void ChangeState(ViewState state)
        {
            Trace.WriteLine($"FlyoutViewModel ChangeState {state}");
            var oldState = State;

            State = state;
            StateChanged(this, _expandOnCloseThenOpen ? CloseReason.CloseThenOpen : CloseReason.Normal);

            if (state == ViewState.Open)
            {
                _mainViewModel.OnTrayFlyoutShown();

                if (_closedOnOpen)
                {
                    _closedOnOpen = false;
                    BeginClose();
                }
            }
            else if (state == ViewState.Hidden)
            {
                if (_expandOnCloseThenOpen)
                {
                    _expandOnCloseThenOpen = false;

                    DoExpandCollapse();
                    BeginOpen();
                }
            }
            else if (state == ViewState.Closing_Stage1)
            {
                _mainViewModel.OnTrayFlyoutHidden();

                if (IsShowingModalDialog)
                {
                    CollapseApp();
                }
            }
            else if (state == ViewState.Closing_Stage2)
            {
                _hideTimer.Start();
            }
        }

        public void BeginExpandApp(AppItemViewModel vm, UIElement container)
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

        public void BeginOpen()
        {
            if (State == ViewState.Hidden)
            {
                ChangeState(ViewState.Opening);
            }
        }

        public void BeginClose(CloseReason reason = CloseReason.Normal)
        {
            if (State == ViewState.Open)
            {
                if (reason == CloseReason.CloseThenOpen)
                {
                    _expandOnCloseThenOpen = true;
                }

                ChangeState(ViewState.Closing_Stage1);
            }
            else if (State == ViewState.Opening)
            {
                _closedOnOpen = true;
            }
        }

        private void OpenFlyout()
        {
            if (State == ViewState.Closing_Stage2)
            {
                return;
            }

            switch (State)
            {
                case ViewState.Hidden:
                    BeginOpen();
                    break;
                case ViewState.Open:
                    BeginClose();
                    break;
            }
        }
    }
}
