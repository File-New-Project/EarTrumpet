using EarTrumpet_Actions.DataModel.Actions;

namespace EarTrumpet_Actions.ViewModel.Actions
{
    class InvokeAddonCommandActionViewModel : PartViewModel
    {
        public OptionViewModel Option { get; set; }

        InvokeAddonCommandAction _part;
        public InvokeAddonCommandActionViewModel(InvokeAddonCommandAction part) : base(part)
        {
            _part = part;
            Option = new OptionViewModel(part);
            Option.PropertyChanged += (_, __) => UpdateDescription();
            UpdateDescription();
        }
    }
}
