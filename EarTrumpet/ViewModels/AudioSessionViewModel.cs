using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using System.Collections.ObjectModel;

namespace EarTrumpet.ViewModels
{
    public class AudioSessionViewModel : BindableBase
    {
        IStreamWithVolumeControl _stream;

        public AudioSessionViewModel(IStreamWithVolumeControl stream)
        {
            _stream = stream;
            _stream.PropertyChanged += Stream_PropertyChanged;
            Children = new ObservableCollection<AudioSessionViewModel>();
            Children.Add(this);
        }

        ~AudioSessionViewModel()
        {
            _stream.PropertyChanged -= Stream_PropertyChanged;
        }

        private void Stream_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(e.PropertyName);
        }

        public virtual string DisplayName => _stream.DisplayName;
        public string Id => _stream.Id;
        public IAudioDeviceSession Session => (IAudioDeviceSession)_stream;
        public ObservableCollection<AudioSessionViewModel> Children { get; protected set; }

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
        public float PeakValue
        {
            get
            {
                float ret = _stream.PeakValue;
                if (Children != null)
                {
                    foreach(var child in Children)
                    {
                        if (child == this) continue;

                        var newValue = child.PeakValue;
                        if (newValue > ret)
                        {
                            ret = newValue;
                        }
                    }
                }
                else
                {
                    ret = _stream.PeakValue;
                }
                return ret;
            }
        }

        public virtual void TriggerPeakCheck()
        {
            RaisePropertyChanged(nameof(PeakValue));

            if (Children != null)
            {
                foreach(var child in Children)
                {
                    if (child != this)
                    {
                        child.TriggerPeakCheck();
                    }
                }
            }
        }
    }
}
