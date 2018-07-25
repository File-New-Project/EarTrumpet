using EarTrumpet_Actions.DataModel.Conditions;

namespace EarTrumpet_Actions.ViewModel.Conditions
{
    class ProcessConditionViewModel : PartViewModel
    {
        public OptionViewModel Option { get; }

        public TextViewModel Text { get; }

        public ProcessConditionViewModel(ProcessCondition condition) : base(condition)
        {
            Option = new OptionViewModel(condition);
            Option.PropertyChanged += (_, __) => UpdateDescription();
            Text = new TextViewModel(condition);
            Text.PropertyChanged += (_, __) => UpdateDescription();
            UpdateDescription();
        }
    }
}
