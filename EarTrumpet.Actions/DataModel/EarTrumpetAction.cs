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

        public void Loaded()
        {
            Triggers.CollectionChanged += TriggerCollectionChanged;
            
            foreach(var t in Triggers)
            {
                t.Triggered += Trigger_Triggered;
                t.Loaded();
            }

            foreach (var t in Actions)
            {
                t.Loaded();
            }

            foreach (var t in Conditions)
            {
                t.Loaded();
            }
        }

        public void ManualTrigger() => Trigger_Triggered();

        private void Trigger_Triggered()
        {
            bool conditionsAreMet = true;
            foreach(var c in Conditions)
            {
                if (!c.IsMet())
                {
                    conditionsAreMet = false;
                    break;
                }
            }

            if (conditionsAreMet)
            {
                foreach(var a in Actions)
                {
                    a.Invoke();
                }
            }
        }

        private void TriggerCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    Register((BaseTrigger)e.NewItems[0]);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    Unregister((BaseTrigger)e.OldItems[0]);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void Register(BaseTrigger p)
        {
            p.Triggered += Trigger_Triggered;
        }

        private void Unregister(BaseTrigger p)
        {
            p.Triggered -= Trigger_Triggered;
        }
    }
}
