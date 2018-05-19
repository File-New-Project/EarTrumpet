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
            _stream.PropertyChanged += Stream_PropertyChanged;
        }

        ~AudioSessionViewModel()
        {
            _stream.PropertyChanged -= Stream_PropertyChanged;
        }

        private void Stream_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(e.PropertyName);
        }

        public string Id => _stream.Id;
        public IAudioDeviceSession Session => (IAudioDeviceSession)_stream;

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
        public virtual float PeakValue => _stream.PeakValue;

        public virtual void TriggerPeakCheck()
        {
            RaisePropertyChanged(nameof(PeakValue));
        }
    }
}
