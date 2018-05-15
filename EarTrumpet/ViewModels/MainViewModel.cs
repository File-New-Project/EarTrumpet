using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows;

namespace EarTrumpet.ViewModels
{
    public enum ViewState
    {
        NotReady,
        Hidden,
        Opening,
        Opening_CloseRequested,
        Open,
        Closing,
    }

    public class AppExpandedEventArgs
    {
        public UIElement Container;
        public AppItemViewModel ViewModel;
    }

    public class MainViewModel : BindableBase
    {
        public static MainViewModel Instance { get; private set; }

        public bool IsExpanded { get; private set; }
        public bool CanExpand => AllDevices.Count > 1;
        public bool IsEmpty => Devices.Count == 0;

        public ObservableCollection<DeviceViewModel> Devices { get; private set; }
        public ObservableCollection<DeviceViewModel> AllDevices { get; private set; }

        public string ExpandText => CanExpand ? (IsExpanded ? "\ue011" : "\ue010") : "";

        public ViewState State { get; private set; }

        public bool IsShowingModalDialog { get; private set; }

        public ObservableCollection<SimpleAudioDeviceViewModel> PlaybackDevices
        {
            get
            {
                var ret = new ObservableCollection<SimpleAudioDeviceViewModel>();
                ret.Add(DefaultPlaybackDevice);

                foreach (var device in AllDevices)
                {
                    ret.Add(new SimpleAudioDeviceViewModel { DisplayName = device.Device.DisplayName, Id = device.Device.Id });
                }

                return ret;
            }
        }
        public SimpleAudioDeviceViewModel DefaultPlaybackDevice { get; private set; }

        public event EventHandler<AppExpandedEventArgs> AppExpanded = delegate { };
        public event EventHandler<object> WindowSizeInvalidated = delegate { };
        public event EventHandler<object> AppCollapsed = delegate { };
        public event EventHandler<ViewState> StateChanged = delegate { };

        private readonly IAudioDeviceManager _deviceService;



        private readonly Timer _peakMeterTimer;
        private AppItemViewModel _expandedApp;

        public MainViewModel(IAudioDeviceManager deviceService)
        {
            State = ViewState.NotReady;

            Debug.Assert(Instance == null);
            Instance = this;

            _deviceService = deviceService;
            _deviceService.DefaultPlaybackDeviceChanged += _deviceService_DefaultPlaybackDeviceChanged;
            _deviceService.Devices.CollectionChanged += Devices_CollectionChanged;

            Devices = new ObservableCollection<DeviceViewModel>();
            AllDevices = new ObservableCollection<DeviceViewModel>();

            DefaultPlaybackDevice = new SimpleAudioDeviceViewModel { DisplayName = EarTrumpet.Properties.Resources.DefaultDeviceText, IsDefault = true, };

            Devices_CollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

            _peakMeterTimer = new Timer(1000 / 30);
            _peakMeterTimer.AutoReset = true;
            _peakMeterTimer.Elapsed += PeakMeterTimer_Elapsed;
        }

        private void AddDevice(IAudioDevice device)
        {
            var newDevice = new DeviceViewModel(_deviceService, device);
            AllDevices.Add(newDevice);

            if (IsExpanded || Devices.Count == 0)
            {
                Devices.Insert(0, newDevice);
                InvalidateWindowSize();
            }
        }

        private void _deviceService_DefaultPlaybackDeviceChanged(object sender, IAudioDevice e)
        {
            // no longer any device
            if (e == null) return;

            var foundDevice = Devices.FirstOrDefault(d => d.Device.Id == e.Id);
            if (foundDevice != null)
            {
                // Move to bottom.
                Devices.Move(Devices.IndexOf(foundDevice), Devices.Count - 1);
            }
            else
            {
                var foundAllDevice = AllDevices.FirstOrDefault(d => d.Device.Id == e.Id);
                if (foundAllDevice != null)
                {
                    Devices.Clear();
                    Devices.Add(foundAllDevice);
                    InvalidateWindowSize();
                }
            }
        }

        private void Devices_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddDevice((IAudioDevice)e.NewItems[0]);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    var removed = ((IAudioDevice)e.OldItems[0]).Id;

                    var existing = Devices.FirstOrDefault(d => d.Device.Id == removed);
                    if (existing != null)
                    {
                        Devices.Remove(existing);
                    }

                    var allExisting = AllDevices.FirstOrDefault(d => d.Device.Id == removed);
                    if (allExisting != null)
                    {
                        AllDevices.Remove(allExisting);
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:

                    AllDevices.Clear();
                    Devices.Clear();

                    foreach (var device in _deviceService.Devices)
                    {
                        AddDevice(device);
                    }

                    _deviceService_DefaultPlaybackDeviceChanged(null, _deviceService.DefaultPlaybackDevice);
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
            InvalidateWindowSize();
        }

        private void PeakMeterTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var device in Devices)
            {
                device.TriggerPeakCheck();
            }
        }

        public void DoExpandCollapse()
        {
            IsExpanded = !IsExpanded;

            if (IsExpanded)
            {
                // Add any that aren't existing.
                foreach (var device in AllDevices)
                {
                    if (!Devices.Contains(device))
                    {
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

                    if (device.Device.Id != _deviceService.DefaultPlaybackDevice.Id)
                    {
                        Devices.Remove(device);
                    }
                }
            }

            RaisePropertyChanged(nameof(IsExpanded));
            RaisePropertyChanged(nameof(ExpandText));
            InvalidateWindowSize();
        }

        private void InvalidateWindowSize()
        {
            WindowSizeInvalidated?.Invoke(this, null);
        }

        public void ChangeState(ViewState state)
        {
            var oldState = State;

            State = state;
            StateChanged(this, state);

            if (state == ViewState.Open)
            {
                _peakMeterTimer.Start();

                if (oldState == ViewState.Opening_CloseRequested)
                {
                    BeginClose();
                }
            }
            else if (state == ViewState.Hidden)
            {
                _peakMeterTimer.Stop();

                if (_expandedApp != null)
                {
                    OnAppCollapsed();
                }
            }
        }

        public void OnAppExpanded(AppItemViewModel vm, UIElement container)
        {
            if (_expandedApp != null)
            {
                OnAppCollapsed();
            }

            _expandedApp = vm;
            _expandedApp.IsExpanded = true;

            AppExpanded?.Invoke(this, new AppExpandedEventArgs { Container = container, ViewModel = vm });

            IsShowingModalDialog = true;
            RaisePropertyChanged(nameof(IsShowingModalDialog));
        }

        public void OnAppCollapsed()
        {
            if (_expandedApp != null)
            {
                _expandedApp.IsExpanded = false;
                _expandedApp = null;

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

            // NotReady - Ignore, can't do anything.
            // Opening - Ignore. Already opening.
            // Opening_CloseRequested - Ignore, already opening and then closing.
            // Open - We're already open.
            // Closing - Drop event. Not worth the complexity?
        }

        public void BeginClose()
        {
            if (State == ViewState.Open)
            {
                ChangeState(ViewState.Closing);
            }
            else if (State == ViewState.Opening)
            {
                ChangeState(ViewState.Opening_CloseRequested);
            }

            // NotReady - Impossible.
            // Hidden - Nothing to do.
            // Opening_CloseRequested - Nothing to do.
            // Closing - Nothing to do.
        }

        public void MoveAppToDevice(AppItemViewModel app, SimpleAudioDeviceViewModel dev)
        {
            var searchId = dev.Id;
            if (dev.IsDefault)
            {
                searchId = _deviceService.DefaultPlaybackDevice.Id;
            }
            DeviceViewModel oldDevice = AllDevices.First(d => d.Apps.Contains(app));
            DeviceViewModel newDevice = AllDevices.First(d => searchId == d.Device.Id);

            try
            {
                var pids = app.ChildApps.Select(c => c.Session.ProcessId).ToSet();

                foreach (var pid in pids)
                {
                    AudioPolicyConfigService.SetDefaultEndPoint(dev.Id, pid);
                }
                if (oldDevice != newDevice)
                {
                    newDevice.OnAppMovedToDevice(app);
                    oldDevice.OnAppMovedFromDevice(app);
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
