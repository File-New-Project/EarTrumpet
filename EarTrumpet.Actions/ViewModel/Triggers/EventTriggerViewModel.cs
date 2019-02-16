using EarTrumpet.Actions.DataModel.Serialization;

namespace EarTrumpet.Actions.ViewModel.Triggers
{
    class EventTriggerViewModel : PartViewModel
    {
        public OptionViewModel Option { get; }

        public EventTriggerViewModel(EventTrigger trigger) : base(trigger)
        {
            Option = new OptionViewModel(trigger, nameof(trigger.Option));
            Attach(Option);
        }
    }
}
