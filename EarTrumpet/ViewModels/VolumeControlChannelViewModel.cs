using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarTrumpet.ViewModels
{
    public class VolumeControlChannelViewModel : BindableBase
    {
        IStreamWithVolumeControl _stream;
        protected string _displayName;

        public VolumeControlChannelViewModel(IStreamWithVolumeControl stream)
        {
            _stream = stream;
            _displayName = _stream.DisplayName;
            _stream.PropertyChanged += (_, e) =>
            {
                RaisePropertyChanged(e.PropertyName);
            };
        }

        public string DisplayName => _displayName;
        public string Id => _stream.Id;
        public bool IsMuted
        {
            get => _stream.IsMuted;
            set => _stream.IsMuted = value;
        }
        public int Volume
        {
            get => _stream.Volume.ToVolumeInt();
            set => _stream.Volume = value/100f;
        }
        public float PeakValue => _stream.PeakValue;

        public void TriggerPeakCheck()
        {
            RaisePropertyChanged("PeakValue");
        }
    }
}
