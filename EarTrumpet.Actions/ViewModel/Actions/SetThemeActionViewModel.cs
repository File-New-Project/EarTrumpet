using EarTrumpet_Actions.DataModel.Actions;

namespace EarTrumpet_Actions.ViewModel.Actions
{
    class SetThemeActionViewModel : PartViewModel
    {
        public OptionViewModel Option { get; set; }

        SetThemeAction _part;
        public SetThemeActionViewModel(SetThemeAction part) : base(part)
        {
            _part = part;
            Option = new OptionViewModel(part);
            Option.PropertyChanged += (_, __) => UpdateDescription();
            UpdateDescription();
        }
    }
}
