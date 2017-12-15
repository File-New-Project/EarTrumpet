using EarTrumpet.Extensions;
using EarTrumpet.Models;
using EarTrumpet.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
        private readonly EarTrumpetAudioDeviceService _deviceService;
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
                _trayIcon = value;
                RaisePropertyChanged("TrayIcon");
            }
        }

        public TrayViewModel(EarTrumpetAudioDeviceService deviceService)
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
            _deviceService.MasterVolumeChanged += DeviceService_MasterVolumeChanged;

            var defaultDevice = _deviceService.GetAudioDevices().FirstOrDefault(x => x.IsDefault);
            if (!defaultDevice.Equals(default(EarTrumpetAudioDeviceModel)))
            {
                var volume = _deviceService.GetAudioDeviceVolume(defaultDevice.Id);
                UpdateTrayIcon(false, defaultDevice.IsMuted, volume.ToVolumeInt());
            }
            else
            {
                UpdateTrayIcon(true);
            }
        }

        internal void ChangeTrayIcon(bool useOldIcon)
        {
            UserPreferencesService.UseOldIcon = useOldIcon;
            if (UserPreferencesService.UseOldIcon)
            {
                UpdateTrayIcon();
            }
            else
            {
                var defaultDevice = _deviceService.GetAudioDevices().FirstOrDefault(x => x.IsDefault);
                var noDevice = defaultDevice.Equals(default(EarTrumpetAudioDeviceModel));
                var volume = _deviceService.GetAudioDeviceVolume(defaultDevice.Id);
                UpdateTrayIcon(noDevice, defaultDevice.IsMuted, volume.ToVolumeInt());
            }
        }

        private void DeviceService_MasterVolumeChanged(object sender, EarTrumpetAudioDeviceService.MasterVolumeChangedArgs e)
        {
            var defaultDevice = _deviceService.GetAudioDevices().FirstOrDefault(x => x.IsDefault);
            var noDevice = defaultDevice.Equals(default(EarTrumpetAudioDeviceModel));
            UpdateTrayIcon(noDevice, defaultDevice.IsMuted, e.Volume.ToVolumeInt());
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

        public void UpdateTrayIcon(bool noDevice = false, bool isMuted = false, int currentVolume = 100)
        {
            if (UserPreferencesService.UseOldIcon)
            {
                if (TrayIcon == null || _currentIcon != IconId.OriginalIcon)
                { 
                    TrayIcon = _icons[IconId.OriginalIcon];
                    _currentIcon = IconId.OriginalIcon;
                }
                return;
            }

            if (noDevice)
            {
                if (_currentIcon == IconId.NoDevice)
                {
                    return;
                }
                TrayIcon = _icons[IconId.NoDevice];
                _currentIcon = IconId.NoDevice;
            }
            else if (isMuted)
            {
                if (_currentIcon == IconId.Muted)
                {
                    return;
                }
                TrayIcon = _icons[IconId.Muted];
                _currentIcon = IconId.Muted;
            }
            else if (currentVolume == 0)
            {
                if (_currentIcon == IconId.SpeakerZeroBars)
                {
                    return;
                }
                TrayIcon = _icons[IconId.SpeakerZeroBars];
                _currentIcon = IconId.SpeakerZeroBars;
            }
            else if (currentVolume >= 1 && currentVolume < 33)
            {
                if (_currentIcon == IconId.SpeakerOneBar)
                {
                    return;
                }
                TrayIcon = _icons[IconId.SpeakerOneBar];
                _currentIcon = IconId.SpeakerOneBar;
            }
            else if (currentVolume >= 33 && currentVolume < 66)
            {
                if (_currentIcon == IconId.SpeakerTwoBars)
                {
                    return;
                }
                TrayIcon = _icons[IconId.SpeakerTwoBars];
                _currentIcon = IconId.SpeakerTwoBars;
            }
            else if (currentVolume >= 66 && currentVolume <= 100)
            {
                if (_currentIcon == IconId.SpeakerThreeBars)
                {
                    return;
                }
                TrayIcon = _icons[IconId.SpeakerThreeBars];
                _currentIcon = IconId.SpeakerThreeBars;
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

        public void SetDefaultAudioDevice(string id)
        {
            _deviceService.SetDefaultAudioDevice(id);
        }

        public List<EarTrumpetAudioDeviceModel> GetAudioDevices()
        {
            return _deviceService.GetAudioDevices().ToList();
        }
    }
}
