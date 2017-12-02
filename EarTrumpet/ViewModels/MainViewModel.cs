using EarTrumpet.DataModel;
using System.Timers;
using System.Windows;

namespace EarTrumpet.ViewModels
{
    public class MainViewModel : BindableBase
    {
        public DeviceViewModel DefaultDevice { get; private set; }

        public Visibility ListVisibility { get; private set; }
        public Visibility NoAppsPaneVisibility { get; private set; }
        public Visibility DeviceVisibility { get; private set; }

        public string NoItemsContent { get; private set; }

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

        private readonly AudioDeviceManager _deviceService;
        Timer _peakMeterTimer;

        public MainViewModel(AudioDeviceManager deviceService)
        {
            _deviceService = deviceService;
            DefaultDevice = new DeviceViewModel(_deviceService.VirtualDefaultDevice);
            _deviceService.VirtualDefaultDevice.PropertyChanged += VirtualDefaultDevice_PropertyChanged;

            UpdateInterfaceState();

            _peakMeterTimer = new Timer(1000 / 30);
            _peakMeterTimer.AutoReset = true;
            _peakMeterTimer.Elapsed += PeakMeterTimer_Elapsed;
        }

        private void PeakMeterTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DefaultDevice.TriggerPeakCheck();
        }

        private void VirtualDefaultDevice_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsDevicePresent")
            {
                UpdateInterfaceState();
            }
        }

        public void UpdateInterfaceState()
        {
            ListVisibility = DefaultDevice.Apps.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            NoAppsPaneVisibility = DefaultDevice.Apps.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            NoItemsContent = !_deviceService.VirtualDefaultDevice.IsDevicePresent ? Properties.Resources.NoDevicesPanelContent : Properties.Resources.NoAppsPanelContent;
            DeviceVisibility = _deviceService.VirtualDefaultDevice.IsDevicePresent ? Visibility.Visible : Visibility.Collapsed;

            RaisePropertyChanged("ListVisibility");
            RaisePropertyChanged("NoAppsPaneVisibility");
            RaisePropertyChanged("NoItemsContent");
            RaisePropertyChanged("DeviceVisibility");
        }
    }
}
