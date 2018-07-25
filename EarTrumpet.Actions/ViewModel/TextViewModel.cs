using EarTrumpet_Actions.DataModel;
using System.ComponentModel;

namespace EarTrumpet_Actions.ViewModel
{
    class TextViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string PromptText => _part.PromptText;

        public string Text
        {
            get => _part.Text;
            set
            {
                _part.Text = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
            }
        }

        private IPartWithText _part;
        public TextViewModel(IPartWithText part)
        {
            _part = part;
        }
    }
}
