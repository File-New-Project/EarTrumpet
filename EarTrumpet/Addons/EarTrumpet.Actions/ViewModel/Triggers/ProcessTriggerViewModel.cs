using EarTrumpet.Actions.DataModel.Serialization;

namespace EarTrumpet.Actions.ViewModel.Triggers
{
    class ProcessTriggerViewModel : PartViewModel
    {
        public OptionViewModel Option { get; }
        public TextViewModel Text { get; }

        public ProcessTriggerViewModel(ProcessTrigger trigger) : base(trigger)
        {
            Option = new OptionViewModel(trigger, nameof(trigger.Option));
            Text = new TextViewModel(trigger);

            Attach(Option);
            Attach(Text);
        }
    }
}
