using EarTrumpet_Actions.DataModel;
using EarTrumpet_Actions.DataModel.Actions;
using EarTrumpet_Actions.DataModel.Conditions;
using EarTrumpet_Actions.DataModel.Triggers;
using EarTrumpet_Actions.ViewModel.Actions;
using EarTrumpet_Actions.ViewModel.Conditions;
using EarTrumpet_Actions.ViewModel.Triggers;
using System;
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
            get => _action.DisplayName;
            set
            {
                if (DisplayName != value)
                {
                    _action.DisplayName = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayName)));
                }
            }
        }

        public ObservableCollection<PartViewModel> Triggers { get; }
        public ObservableCollection<PartViewModel> Conditions { get;}
        public ObservableCollection<PartViewModel> Actions { get; }

        private readonly EarTrumpetAction _action;

        public EarTrumpetActionViewModel(EarTrumpetAction action)
        {
            _action = action;
            DisplayName = _action.DisplayName;
            Triggers = new ObservableCollection<PartViewModel>(action.Triggers.Select(t => CreatePartViewModel(t)));
            Conditions = new ObservableCollection<PartViewModel>(action.Conditions.Select(t => CreatePartViewModel(t)));
            Actions = new ObservableCollection<PartViewModel>(action.Actions.Select(t => CreatePartViewModel(t)));

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

        private PartViewModel CreatePartViewModel(Part part)
        {
            if (part is EventTrigger)
            {
                return new EventTriggerViewModel((EventTrigger)part);
            }
            else if (part is ProcessTrigger)
            {
                return new ProcessTriggerViewModel((ProcessTrigger)part);
            }
            else if (part is HotkeyTrigger)
            {
                return new HotkeyTriggerViewModel((HotkeyTrigger)part);
            }
            else if (part is AudioDeviceEventTrigger)
            {
                return new AudioDeviceEventTriggerViewModel((AudioDeviceEventTrigger)part);
            }
            else if (part is AudioDeviceSessionEventTrigger)
            {
                return new AudioDeviceSessionEventTriggerViewModel((AudioDeviceSessionEventTrigger)part);
            }
            else if (part is DefaultPlaybackDeviceCondition)
            {
                return new DefaultPlaybackDeviceConditionViewModel((DefaultPlaybackDeviceCondition)part);
            }
            else if (part is VariableCondition)
            {
                return new VariableConditionViewModel((VariableCondition)part);
            }
            else if (part is ProcessCondition)
            {
                return new ProcessConditionViewModel((ProcessCondition)part);
            }
            else if (part is SetVariableAction)
            {
                return new SetVariableActionViewModel((SetVariableAction)part);
            }
            else if (part is SetDefaultDeviceAction)
            {
                return new SetDefaultDeviceActionViewModel((SetDefaultDeviceAction)part);
            }
            else if (part is ChangeDeviceVolumeAction)
            {
                return new ChangeDeviceVolumeActionViewModel((ChangeDeviceVolumeAction)part);
            }
            else if (part is ChangeAppVolumeAction)
            {
                return new ChangeAppVolumeActionViewModel((ChangeAppVolumeAction)part);
            }

            throw new NotImplementedException();
        }
    }
}
