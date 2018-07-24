using EarTrumpet_Actions.DataModel;
using EarTrumpet_Actions.DataModel.Actions;
using EarTrumpet_Actions.DataModel.Conditions;
using EarTrumpet_Actions.DataModel.Triggers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace EarTrumpet_Actions.ViewModel
{
    public class EarTrumpetActionViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string DisplayName
        {
            get => _displayName;
            set
            {
                if (_displayName != value)
                {
                    _displayName = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayName)));
                }
            }
        }

        public ObservableCollection<PartViewModel> Triggers { get; }
        public ObservableCollection<PartViewModel> Conditions { get;}
        public ObservableCollection<PartViewModel> Actions { get; }

        private string _displayName;
        private readonly EarTrumpetAction _action;

        public EarTrumpetActionViewModel(EarTrumpetAction action)
        {
            _action = action;
            DisplayName = _action.DisplayName;
            Triggers = new ObservableCollection<PartViewModel>(action.Triggers.Select(t => new PartViewModel(t)));
            Conditions = new ObservableCollection<PartViewModel>(action.Conditions.Select(t => new PartViewModel(t)));
            Actions = new ObservableCollection<PartViewModel>(action.Actions.Select(t => new PartViewModel(t)));

            Triggers.CollectionChanged += Triggers_CollectionChanged;
            Conditions.CollectionChanged += Conditions_CollectionChanged;
            Actions.CollectionChanged += Actions_CollectionChanged;
        }

        private void Actions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    _action.Actions.Add((BaseAction)((PartViewModel)e.NewItems[0]).Part);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    _action.Actions.Remove((BaseAction)((PartViewModel)e.OldItems[0]).Part);
                    break;
            }
        }

        private void Conditions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    _action.Conditions.Add((BaseCondition)((PartViewModel)e.NewItems[0]).Part);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    _action.Conditions.Remove((BaseCondition)((PartViewModel)e.OldItems[0]).Part);
                    break;
            }
        }

        private void Triggers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    _action.Triggers.Add((BaseTrigger)((PartViewModel)e.NewItems[0]).Part);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    _action.Triggers.Remove((BaseTrigger)((PartViewModel)e.OldItems[0]).Part);
                    break;
            }
        }

        public EarTrumpetAction GetAction()
        {
            _action.DisplayName = DisplayName;
            _action.Triggers = new ObservableCollection<BaseTrigger>(Triggers.Select(t => (BaseTrigger)t.Part));
            _action.Conditions = new ObservableCollection<BaseCondition>(Conditions.Select(t => (BaseCondition)t.Part));
            _action.Actions = new ObservableCollection<BaseAction>(Actions.Select(t => (BaseAction)t.Part));
            return _action;
        }
    }
}
