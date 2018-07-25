using EarTrumpet_Actions.DataModel;
using System.ComponentModel;

namespace EarTrumpet_Actions.ViewModel
{
    class VolumeViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public double Volume
        {
            get => _part.Volume;
            set
            {
                _part.Volume = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Volume)));
            }
        }

        private IPartWithVolume _part;
        public VolumeViewModel(IPartWithVolume part)
        {
            _part = part;
        }
    }
}
