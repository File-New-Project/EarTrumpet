using EarTrumpet_Actions.DataModel.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarTrumpet_Actions.ViewModel.Triggers
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
