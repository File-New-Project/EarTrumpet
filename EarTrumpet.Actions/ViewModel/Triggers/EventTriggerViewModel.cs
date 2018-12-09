using EarTrumpet_Actions.DataModel.Triggers;

namespace EarTrumpet_Actions.ViewModel.Triggers
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
