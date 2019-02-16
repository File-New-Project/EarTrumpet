using System;
using System.Collections.ObjectModel;

namespace EarTrumpet.Actions.DataModel.Serialization
{
    public class EarTrumpetAction
    {
        public string DisplayName { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();
        public ObservableCollection<BaseTrigger> Triggers { get; set; } = new ObservableCollection<BaseTrigger>();
        public ObservableCollection<BaseCondition> Conditions { get; set; } = new ObservableCollection<BaseCondition>();
        public ObservableCollection<BaseAction> Actions { get; set; } = new ObservableCollection<BaseAction>();
    }
}
