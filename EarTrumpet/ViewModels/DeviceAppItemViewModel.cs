using EarTrumpet.Extensions;
using EarTrumpet.Models;
using EarTrumpet.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace EarTrumpet.ViewModels
{
    public class DeviceAppItemViewModel : BindableBase
    {
        private readonly IAudioMixerViewModelCallback _callback;
        private readonly EarTrumpetAudioDeviceModel _device;

        public string Id { get; set; }
        public string DisplayName { get; set; }
        public ImageSource Icon { get; set; }
        public double IconHeight { get; set; }
        public double IconWidth { get; set; }

        private int _volume;
        public int Volume
        {
            get
            {
                return _volume;
            }
            set
            {
                if (_volume == value) return;
                _volume = value;
                _callback.SetDeviceVolume(_device, _volume / 100.0f);
                RaisePropertyChanged("Volume");
            }
        }
        public SolidColorBrush Background { get; set; }

        private bool _isMuted = false;
        public bool IsMuted
        {
            get
            {
                return _isMuted;
            }
            set
            {
                if (_isMuted != value)
                {
                    _isMuted = value;

                    _callback.SetDeviceMute(_device, _isMuted);
                    RaisePropertyChanged("IsMuted");
                }
            }
        }

        public DeviceAppItemViewModel(IAudioMixerViewModelCallback callback, EarTrumpetAudioDeviceModel device, float volume)
        {
            IconHeight = IconWidth = 32;

            _device = device;
            _volume = Convert.ToInt32(Math.Round((volume * 100), MidpointRounding.AwayFromZero));
            _isMuted = device.IsMuted;
            _callback = callback;
            DisplayName = device.DisplayName;
            Id = device.Id;            
        }

        public void UpdateFromOther(DeviceAppItemViewModel other)
        {
            if (_volume == other.Volume) return;
            _volume = other.Volume;
            _isMuted = other.IsMuted;
            RaisePropertyChanged("Volume");
            RaisePropertyChanged("IsMuted");
        }

        public bool IsSame(DeviceAppItemViewModel other)
        {
            return other.Id == Id;
        }
    }
}
