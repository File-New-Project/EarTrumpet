using EarTrumpet_Actions.DataModel.Triggers;
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
            Option = new OptionViewModel(trigger);
            Option.PropertyChanged += (_, __) => UpdateDescription();
            Text = new TextViewModel(trigger);
            Text.PropertyChanged += (_, __) => UpdateDescription();

            UpdateDescription();
        }
    }
}
