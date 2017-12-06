using EarTrumpet.DataModel;
using EarTrumpet.Services;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows;

namespace EarTrumpet.ViewModels
{
    public class MainViewModel : BindableBase
    {
        public DeviceViewModel DefaultDevice { get; private set; }

        public Visibility ListVisibility => DefaultDevice.Apps.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        public Visibility NoAppsPaneVisibility => DefaultDevice.Apps.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        public Visibility DeviceVisibility => _deviceService.VirtualDefaultDevice.IsDevicePresent ? Visibility.Visible : Visibility.Collapsed;

        public string NoItemsContent => !_deviceService.VirtualDefaultDevice.IsDevicePresent ? Properties.Resources.NoDevicesPanelContent : Properties.Resources.NoAppsPanelContent;

        public Visibility ExpandedPaneVisibility { get; private set; }

        public string ExpandText => ExpandedPaneVisibility == Visibility.Visible ? "\ue010" : "\ue011";

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

        private readonly IAudioDeviceManager _deviceService;
        private readonly Timer _peakMeterTimer;

        public MainViewModel(IAudioDeviceManager deviceService)
        {
            _deviceService = deviceService;
            _deviceService.SessionCreated += DeviceManager_SessionCreated;
            _deviceService.VirtualDefaultDevice.PropertyChanged += VirtualDefaultDevice_PropertyChanged;

            DefaultDevice = new DeviceViewModel(_deviceService.VirtualDefaultDevice);

            ExpandedPaneVisibility = Visibility.Collapsed;
            UpdateInterfaceState();

            _peakMeterTimer = new Timer(1000 / 30);
            _peakMeterTimer.AutoReset = true;
            _peakMeterTimer.Elapsed += PeakMeterTimer_Elapsed;

            // Enumerate apps and set startup values.
            foreach(var dev in _deviceService.Devices)
            {
                foreach(var session in dev.Sessions)
                {
                    DeviceManager_SessionCreated(null, session);
                }
            }
        }

        private void DeviceManager_SessionCreated(object sender, IAudioDeviceSession e)
        {
            var appId = AppResolverService.GetAppIdForProcess((uint)e.ProcessId);
            var appSetting = SettingsService.DefaultApps.FirstOrDefault(d => d.Id == appId);
            if (appSetting != null)
            {
                Debug.WriteLine("Default Session Applied: " + e.DisplayName);
                e.IsMuted = appSetting.IsMuted;
                e.Volume = appSetting.Volume;
            }
        }

        private void PeakMeterTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DefaultDevice.TriggerPeakCheck();
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
    }
}
