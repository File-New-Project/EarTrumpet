using EarTrumpet.DataModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

    public class MainViewModel : BindableBase
    {
        public static MainViewModel Instance { get; private set; }

        public DeviceViewModel DefaultDevice { get; private set; }

        public ObservableCollection<DeviceViewModel> Devices { get; private set; }

        public Visibility ListVisibility => DefaultDevice.Apps.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        public Visibility NoAppsPaneVisibility => DefaultDevice.Apps.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        public Visibility DeviceVisibility => _deviceService.VirtualDefaultDevice.IsDevicePresent ? Visibility.Visible : Visibility.Collapsed;

        public string NoItemsContent => !_deviceService.VirtualDefaultDevice.IsDevicePresent ? Properties.Resources.NoDevicesPanelContent : Properties.Resources.NoAppsPanelContent;

        public Visibility ExpandedPaneVisibility { get; private set; }

        public string ExpandText => ExpandedPaneVisibility == Visibility.Collapsed ? "\ue010" : "\ue011";

        bool _isVisible = false;
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    if (_isVisible)
                    {
                        _peakMeterTimer.Start();
                    }
                    else
                    {
                        _peakMeterTimer.Stop();
                    }
                }
            }
        }

        public ViewState State { get; private set; }

        public static AppItemViewModel ExpandedApp { get; set; }

        public event EventHandler<ViewState> StateChanged = delegate { };

        private readonly IAudioDeviceManager _deviceService;
        private readonly Timer _peakMeterTimer;
        private List<IAudioDevice> _allDevices = new List<IAudioDevice>();

        public MainViewModel(IAudioDeviceManager deviceService)
        {
            State = ViewState.NotReady;
            Instance = this;

            _deviceService = deviceService;
            _deviceService.VirtualDefaultDevice.PropertyChanged += VirtualDefaultDevice_PropertyChanged;
            _deviceService.DefaultPlaybackDeviceChanged += _deviceService_DefaultPlaybackDeviceChanged;
            _deviceService.Devices.CollectionChanged += Devices_CollectionChanged;

            Devices = new ObservableCollection<DeviceViewModel>();
            DefaultDevice = new DeviceViewModel(_deviceService, _deviceService.VirtualDefaultDevice);

            ExpandedPaneVisibility = Visibility.Collapsed;
            UpdateInterfaceState();
            Devices_CollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

            _peakMeterTimer = new Timer(1000 / 30);
            _peakMeterTimer.AutoReset = true;
            _peakMeterTimer.Elapsed += PeakMeterTimer_Elapsed;
        }

        private void _deviceService_DefaultPlaybackDeviceChanged(object sender, IAudioDevice e)
        {
            foreach(var device in _allDevices)
            {
                CheckApplicability(device);
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
                    _allDevices.Remove((IAudioDevice)e.OldItems[0]);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _allDevices.Clear();
                    foreach (var device in _deviceService.Devices)
                    {
                        AddDevice(device);
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        void AddDevice(IAudioDevice device)
        {
            _allDevices.Add(device);

            CheckApplicability(device);
        }

        void CheckApplicability(IAudioDevice device)
        {
            if (_deviceService.DefaultPlaybackDevice != device)
            {
                if (!Devices.Any(d => d.Device.Id == device.Id))
                {
                    Devices.Add(new DeviceViewModel(_deviceService, device));
                    return;
                }
            }

            var existing = Devices.FirstOrDefault(d => d.Device.Id == device.Id);
            if (existing != null)
            {
                Devices.Remove(existing);
            }
        }

        private void PeakMeterTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DefaultDevice.TriggerPeakCheck();

            foreach (var device in Devices)
            {
                device.TriggerPeakCheck();
            }
        }

        private void VirtualDefaultDevice_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_deviceService.VirtualDefaultDevice.IsDevicePresent))
            {
                UpdateInterfaceState();
            }
        }

        public void UpdateInterfaceState()
        {
            RaisePropertyChanged(nameof(ListVisibility));
            RaisePropertyChanged(nameof(NoAppsPaneVisibility));
            RaisePropertyChanged(nameof(NoItemsContent));
            RaisePropertyChanged(nameof(DeviceVisibility));
        }

        public void DoExpandCollapse()
        {
            ExpandedPaneVisibility = ExpandedPaneVisibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            RaisePropertyChanged(nameof(ExpandedPaneVisibility));
            RaisePropertyChanged(nameof(ExpandText));
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
            }
        }

        public void BeginOpen()
        {
            if (State == ViewState.Hidden)
            {
                ChangeState(ViewState.Opening);
            }
        }

        public void BeginClose()
        {
            if (State == ViewState.Open)
            {
                ChangeState(ViewState.Closing);
            } else if (State == ViewState.Opening)
            {
                ChangeState(ViewState.Opening_CloseRequested);
            }
        }
    }
}
