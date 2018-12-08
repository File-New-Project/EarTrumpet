using EarTrumpet.UI.ViewModels;
using EarTrumpet_Actions.DataModel;

namespace EarTrumpet_Actions.ViewModel
{
    class TextViewModel : BindableBase
    {
        public string PromptText => _part.PromptText;

        public string Text
        {
            get => _part.Text;
            set
            {
                _part.Text = value;
                RaisePropertyChanged(nameof(Text));
            }
        }

        private IPartWithText _part;
        public TextViewModel(IPartWithText part)
        {
            _part = part;
        }

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(_part.Text))
            {
                return _part.EmptyText;
            }
            else
            { 
                return _part.Text;
            }
        }
    }
}
