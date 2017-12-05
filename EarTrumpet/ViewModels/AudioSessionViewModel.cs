using EarTrumpet.DataModel;
using EarTrumpet.Extensions;

namespace EarTrumpet.ViewModels
{
    public class AudioSessionViewModel : BindableBase
    {
        IStreamWithVolumeControl _stream;

        public AudioSessionViewModel(IStreamWithVolumeControl stream)
        {
            _stream = stream;
            _stream.PropertyChanged += (_, e) =>
            {
                RaisePropertyChanged(e.PropertyName);
            };
        }

        public virtual string DisplayName => _stream.DisplayName;
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
            RaisePropertyChanged(nameof(PeakValue));
        }
    }
}
