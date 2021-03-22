using EarTrumpet.Actions.DataModel;
using System;

namespace EarTrumpet.Actions.ViewModel
{
    class TextViewModel : BindableBase
    {
        public string PromptText => ResolveResource("PromptText");

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
                return ResolveResource("EmptyText");
            }
            else
            { 
                return _part.Text;
            }
        }

        private string ResolveResource(string suffix)
        {
            var res = $"{_part.GetType().Name}_{suffix}";
            var ret = Properties.Resources.ResourceManager.GetString(res);
            if (string.IsNullOrWhiteSpace(ret))
            {
                throw new NotImplementedException($"Missing resource: {res}");
            }
            return ret;
        }
    }
}
