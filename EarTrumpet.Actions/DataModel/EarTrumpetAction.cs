using EarTrumpet_Actions.DataModel.Actions;
using EarTrumpet_Actions.DataModel.Conditions;
using EarTrumpet_Actions.DataModel.Triggers;
using System;
using System.Collections.ObjectModel;

namespace EarTrumpet_Actions.DataModel
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

        public override string ToString()
        {
            return $"Action {DisplayName} with {Triggers.Count} triggers, {Conditions.Count} conditions and {Actions.Count} actions";
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}
