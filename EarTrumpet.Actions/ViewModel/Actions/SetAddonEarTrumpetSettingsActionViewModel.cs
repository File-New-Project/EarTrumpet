using EarTrumpet_Actions.DataModel.Actions;

namespace EarTrumpet_Actions.ViewModel.Actions
{
    class SetAddonEarTrumpetSettingsActionViewModel : PartViewModel
    {
        public OptionViewModel Option1 { get; set; }
        public OptionViewModel Option2 { get; set; }

        SetAddonEarTrumpetSettingsAction _part;
        public SetAddonEarTrumpetSettingsActionViewModel(SetAddonEarTrumpetSettingsAction part) : base(part)
        {
            _part = part;
            Option1 = new OptionViewModel(part);
            Option1.PropertyChanged += (_, __) => UpdateDescription();
            Option2 = new OptionViewModel(part, 1);
            Option2.PropertyChanged += (_, __) => UpdateDescription();
            UpdateDescription();
        }
    }
}
