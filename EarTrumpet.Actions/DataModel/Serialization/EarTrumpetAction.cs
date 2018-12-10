using System;
using System.Collections.ObjectModel;

namespace EarTrumpet_Actions.DataModel.Serialization
{
    public class EarTrumpetAction
    {
        public string DisplayName { get; set; }
        public Guid Id { get; set; }
        public ObservableCollection<BaseTrigger> Triggers { get; set; }
        public ObservableCollection<BaseCondition> Conditions { get; set; }
        public ObservableCollection<BaseAction> Actions { get; set; }

        public EarTrumpetAction()
        {
            Id = Guid.NewGuid();
            Triggers = new ObservableCollection<BaseTrigger>();
            Conditions = new ObservableCollection<BaseCondition>();
            Actions = new ObservableCollection<BaseAction>();
        }
    }
}
