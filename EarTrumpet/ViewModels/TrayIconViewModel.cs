using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;

namespace EarTrumpet.ViewModels
{
    public class TrayViewModel : BindableBase
    {
        public enum IconId
        {
            Muted = 120,
            SpeakerZeroBars = 121,
            SpeakerOneBar = 122,
            SpeakerTwoBars = 123,
            SpeakerThreeBars = 124,
            NoDevice = 125,
            OriginalIcon
        }

        private readonly string _trayIconPath = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\System32\SndVolSSO.dll");
        private Icon _trayIcon;
        private readonly IAudioDeviceManager _deviceService;
        private AppServiceConnection _appServiceConnection;
        private readonly Dictionary<IconId, Icon> _icons = new Dictionary<IconId, Icon>();
        private IconId _currentIcon = IconId.OriginalIcon;

        public Icon TrayIcon
        {
            get
            {
                return _trayIcon;
            }
            set
            {
                if (_trayIcon != value)
                {
                    _trayIcon = value;
                    RaisePropertyChanged("TrayIcon");
                }
            }
        }

        public TrayViewModel(IAudioDeviceManager deviceService)
        {
            var originalIcon = new Icon(Application.GetResourceStream(new Uri("pack://application:,,,/EarTrumpet;component/Tray.ico")).Stream);
            _icons.Add(IconId.OriginalIcon, originalIcon);
            try
            {
                _icons.Add(IconId.NoDevice, IconService.GetIconFromFile(_trayIconPath, (int)IconId.NoDevice));
                _icons.Add(IconId.Muted, IconService.GetIconFromFile(_trayIconPath, (int)IconId.Muted));
                _icons.Add(IconId.SpeakerZeroBars, IconService.GetIconFromFile(_trayIconPath, (int)IconId.SpeakerZeroBars));
                _icons.Add(IconId.SpeakerOneBar, IconService.GetIconFromFile(_trayIconPath, (int)IconId.SpeakerOneBar));
                _icons.Add(IconId.SpeakerTwoBars, IconService.GetIconFromFile(_trayIconPath, (int)IconId.SpeakerTwoBars));
                _icons.Add(IconId.SpeakerThreeBars, IconService.GetIconFromFile(_trayIconPath, (int)IconId.SpeakerThreeBars));
            }
            catch
            {
                _icons.Clear();
                _icons.Add(IconId.NoDevice, originalIcon);
                _icons.Add(IconId.Muted, originalIcon);
                _icons.Add(IconId.SpeakerZeroBars, originalIcon);
                _icons.Add(IconId.SpeakerOneBar, originalIcon);
                _icons.Add(IconId.SpeakerTwoBars, originalIcon);
                _icons.Add(IconId.SpeakerThreeBars, originalIcon);

                _icons.Add(IconId.OriginalIcon, originalIcon);
            }

            _deviceService = deviceService;
            _deviceService.VirtualDefaultDevice.PropertyChanged += VirtualDefaultDevice_PropertyChanged;

            UpdateTrayIcon();
        }

        private void VirtualDefaultDevice_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateTrayIcon();
        }

        private void AppServiceConnectionCompleted(IAsyncOperation<AppServiceConnectionStatus> operation, AsyncStatus asyncStatus)
        {
            var status = operation.GetResults();
            if (status == AppServiceConnectionStatus.Success)
            {
                var secondOperation = _appServiceConnection.SendMessageAsync(null);
                secondOperation.Completed = (_, __) =>
                {
                    _appServiceConnection.Dispose();
                    _appServiceConnection = null;
                };
            }
        }

        void UpdateTrayIcon()
        {
            bool noDevice = !_deviceService.VirtualDefaultDevice.IsDevicePresent;
            bool isMuted = _deviceService.VirtualDefaultDevice.IsMuted;
            int currentVolume = _deviceService.VirtualDefaultDevice.Volume.ToVolumeInt();

            if (noDevice)
            {
                _currentIcon = IconId.NoDevice;
                TrayIcon = _icons[IconId.NoDevice];
                return;
            }
            else if (isMuted)
            {
                _currentIcon = IconId.Muted;
                TrayIcon = _icons[IconId.Muted];
                return;
            }
            else if (currentVolume == 0)
            {
                _currentIcon = IconId.SpeakerZeroBars;
                TrayIcon = _icons[IconId.SpeakerZeroBars];
                return;
            }
            else if (currentVolume >= 1 && currentVolume < 33)
            {
                _currentIcon = IconId.SpeakerOneBar;
                TrayIcon = _icons[IconId.SpeakerOneBar];
                return;
            }
            else if (currentVolume >= 33 && currentVolume < 66)
            {
                _currentIcon = IconId.SpeakerTwoBars;
                TrayIcon = _icons[IconId.SpeakerTwoBars];
                return;
            }
            else if (currentVolume >= 66 && currentVolume <= 100)
            {
                _currentIcon = IconId.SpeakerThreeBars;
                TrayIcon = _icons[IconId.SpeakerThreeBars];
                return;
            }

            if (_currentIcon == IconId.OriginalIcon)
            {
                TrayIcon = _icons[IconId.OriginalIcon];
                _currentIcon = IconId.OriginalIcon;
            }
        }

        public void StartAppServiceAndFeedbackHub()
        {
            if (_appServiceConnection == null)
            {
                _appServiceConnection = new AppServiceConnection();
            }

            _appServiceConnection.AppServiceName = "SendFeedback";
            _appServiceConnection.PackageFamilyName = Package.Current.Id.FamilyName;
            _appServiceConnection.OpenAsync().Completed = AppServiceConnectionCompleted;
        }

        public void CloseAppService()
        {
            if (_appServiceConnection != null)
            {
                _appServiceConnection.Dispose();
            }
        }
    }
}
