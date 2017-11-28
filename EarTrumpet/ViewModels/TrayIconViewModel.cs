using EarTrumpet.Extensions;
using EarTrumpet.Models;
using EarTrumpet.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
        }

        private readonly string _trayIconPath = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\System32\SndVolSSO.dll");
        private Icon _trayIcon;
        private EarTrumpetAudioDeviceService _deviceService;
        private AppServiceConnection _appServiceConnection;


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

        public TrayViewModel(EarTrumpetAudioDeviceService deviceService) {
            
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
            if (noDevice)
            {
                TrayIcon = IconService.GetIconFromFile(_trayIconPath, (int)IconId.NoDevice);
                return;
            }
            if (isMuted)
            {
                TrayIcon = IconService.GetIconFromFile(_trayIconPath, (int)IconId.Muted);
                return;
            }
            if (currentVolume == 0)
            {
                TrayIcon = IconService.GetIconFromFile(_trayIconPath, (int)IconId.SpeakerZeroBars);
                return;
            }
            if (currentVolume >= 1 && currentVolume < 33)
            {
                TrayIcon = IconService.GetIconFromFile(_trayIconPath, (int)IconId.SpeakerOneBar);
                return;
            }
            if (currentVolume >= 33 && currentVolume < 66)
            {
                TrayIcon = IconService.GetIconFromFile(_trayIconPath, (int)IconId.SpeakerTwoBars);
                return;
            }
            if (currentVolume >= 66 && currentVolume <= 100)
            {
                TrayIcon = IconService.GetIconFromFile(_trayIconPath, (int)IconId.SpeakerThreeBars);
                return;
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
