using EarTrumpet.Actions.DataModel.Serialization;

namespace EarTrumpet.Actions.ViewModel.Conditions
{
    class ProcessConditionViewModel : PartViewModel
    {
        public OptionViewModel Option { get; }

        public TextViewModel Text { get; }

        public ProcessConditionViewModel(ProcessCondition condition) : base(condition)
        {
            Option = new OptionViewModel(condition, nameof(condition.Option));
            Text = new TextViewModel(condition);

            Attach(Option);
            Attach(Text);
        }
    }
}
