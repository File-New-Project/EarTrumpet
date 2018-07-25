using EarTrumpet_Actions.DataModel.Actions;

namespace EarTrumpet_Actions.ViewModel.Actions
{
    class SetVariableActionViewModel : PartViewModel
    {
        public OptionViewModel Option { get; }
        public TextViewModel Text { get; }

        public SetVariableActionViewModel(SetVariableAction action) : base(action)
        {
            Option = new OptionViewModel(action);
            Option.PropertyChanged += (_, __) => UpdateDescription();
            Text = new TextViewModel(action);
            Text.PropertyChanged += (_, __) => UpdateDescription();
        }
    }
}
