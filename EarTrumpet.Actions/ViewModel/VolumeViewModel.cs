using EarTrumpet.UI.ViewModels;
using EarTrumpet_Actions.DataModel;

namespace EarTrumpet_Actions.ViewModel
{
    class VolumeViewModel : BindableBase
    {
        public double Volume
        {
            get => _part.Volume;
            set
            {
                _part.Volume = value;
                RaisePropertyChanged(nameof(Volume));
            }
        }

        private IPartWithVolume _part;
        public VolumeViewModel(IPartWithVolume part)
        {
            _part = part;
        }
    }
}
