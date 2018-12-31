using EarTrumpet.DataModel;
using EarTrumpet.Extensions;

namespace EarTrumpet.UI.ViewModels
{
    public class AudioSessionViewModel : BindableBase
    {
        IStreamWithVolumeControl _stream;

        internal AudioSessionViewModel(IStreamWithVolumeControl stream)
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
        public virtual float PeakValue1 => _stream.PeakValue1;
        public virtual float PeakValue2 => _stream.PeakValue2;

        public virtual void UpdatePeakValueForeground()
        {
            RaisePropertyChanged(nameof(PeakValue1));
            RaisePropertyChanged(nameof(PeakValue2));
        }
    }
}
