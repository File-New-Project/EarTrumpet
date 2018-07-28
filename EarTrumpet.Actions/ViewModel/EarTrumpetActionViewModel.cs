using EarTrumpet.Extensibility;
using EarTrumpet.Extensibility.Shared;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using EarTrumpet_Actions.DataModel;
using EarTrumpet_Actions.DataModel.Actions;
using EarTrumpet_Actions.DataModel.Conditions;
using EarTrumpet_Actions.DataModel.Triggers;
using EarTrumpet_Actions.ViewModel.Actions;
using EarTrumpet_Actions.ViewModel.Conditions;
using EarTrumpet_Actions.ViewModel.Triggers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace EarTrumpet_Actions.ViewModel
{
    public class EarTrumpetActionViewModel : BindableBase
    {
        public string Title => DisplayName;

        public ICommand Open { get; set; }
        public ICommand OpenDialog { get; set; }
        public ICommand Remove { get; set; }

        public string DisplayName
        {
            get => _action.DisplayName;
            set
            {
                if (DisplayName != value)
                {
                    _action.DisplayName = value;
                    RaisePropertyChanged(nameof(DisplayName));
                }
            }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    RaisePropertyChanged(nameof(IsExpanded));
                }
            }
        }


        public List<object> AllTriggers
        {
            get
            {
                var hotkeyTriggerViewModel = new HotkeyTriggerViewModel(new HotkeyTrigger { });
                hotkeyTriggerViewModel.SetHotkey = _selectHotkey;

                return new List<object>
                {
                    new EventTriggerViewModel(new EventTrigger{}),
                    new AudioDeviceEventTriggerViewModel(new AudioDeviceEventTrigger{ }),
                    new AudioDeviceSessionEventTriggerViewModel(new AudioDeviceSessionEventTrigger{ }),
                    new ProcessTriggerViewModel(new ProcessTrigger{ }),
                    hotkeyTriggerViewModel,
                };
            }
        }

        public List<PartViewModel> AllConditions
        {
            get
            {
                return new List<PartViewModel>
                {
                    new DefaultPlaybackDeviceConditionViewModel(new DefaultPlaybackDeviceCondition{ }),
                    new ProcessConditionViewModel(new ProcessCondition{ }),
                    new VariableConditionViewModel(new VariableCondition{ }),
                };
            }
        }

        public List<PartViewModel> AllActions
        {
            get
            {
                var ret = new List<PartViewModel>
                {
                    new ChangeAppVolumeActionViewModel(new ChangeAppVolumeAction{ }),
                    new ChangeDeviceVolumeActionViewModel(new ChangeDeviceVolumeAction{ }),
                    new SetDefaultDeviceActionViewModel(new SetDefaultDeviceAction{ }),
                    new SetVariableActionViewModel(new SetVariableAction{ }),
                };

                var addonValues = ServiceBus.GetMany(KnownServices.ValueService);
                if (addonValues != null && addonValues.Any())
                {
                    ret.Add(new SetAddonEarTrumpetSettingsActionViewModel(new SetAddonEarTrumpetSettingsAction()));
                }

                if (ServiceBus.Get("EarTrumpet-Themes") != null)
                {
                    ret.Add(new SetThemeActionViewModel(new SetThemeAction { }));
                }
                return ret;
            }
        }


        private PartViewModel _selectedTrigger;
        public PartViewModel SelectedTrigger
        {
            get => _selectedTrigger;
            set
            {
                if (_selectedTrigger != value)
                {
                    if (_selectedTrigger != null)
                    {
                        _selectedTrigger.IsExpanded = false;
                    }
                    _selectedTrigger = value;
                    if (_selectedTrigger != null)
                    {
                        _selectedTrigger.IsExpanded = true;
                    }
                    RaisePropertyChanged(nameof(SelectedTrigger));
                }
            }
        }

        private PartViewModel _selectedCondition;
        public PartViewModel SelectedCondition
        {
            get => _selectedCondition;
            set
            {
                if (_selectedCondition != value)
                {
                    if (_selectedCondition != null)
                    {
                        _selectedCondition.IsExpanded = false;
                    }
                    _selectedCondition = value;
                    if (_selectedCondition != null)
                    {
                        _selectedCondition.IsExpanded = true;
                    }
                    RaisePropertyChanged(nameof(SelectedCondition));
                }
            }
        }

        private PartViewModel _selectedAction;
        public PartViewModel SelectedAction
        {
            get => _selectedAction;
            set
            {
                if (_selectedAction != value)
                {
                    if (_selectedAction != null)
                    {
                        _selectedAction.IsExpanded = false;
                    }
                    _selectedAction = value;
                    if (_selectedAction != null)
                    {
                        _selectedAction.IsExpanded = true;
                    }
                    RaisePropertyChanged(nameof(SelectedAction));
                }
            }
        }

        public ObservableCollection<PartViewModel> Triggers { get; }
        public ObservableCollection<PartViewModel> Conditions { get;}
        public ObservableCollection<PartViewModel> Actions { get; }

        private readonly EarTrumpetAction _action;
        private ActionsEditorViewModel _parent;
        private ICommand _selectHotkey;

        public EarTrumpetActionViewModel(ActionsEditorViewModel parent, EarTrumpetAction action)
        {
            _parent = parent;
            _action = action;
            DisplayName = _action.DisplayName;

            _selectHotkey = new RelayCommand<HotkeyTriggerViewModel>((vm) =>
            {
                var hotkeyVm = new HotkeySelectViewModel();
                OpenDialog.Execute(hotkeyVm);
                vm.Hotkey = hotkeyVm.Hotkey;
            });

            Triggers = new ObservableCollection<PartViewModel>(action.Triggers.Select(t => CreatePartViewModel(t)));
            Conditions = new ObservableCollection<PartViewModel>(action.Conditions.Select(t => CreatePartViewModel(t)));
            Actions = new ObservableCollection<PartViewModel>(action.Actions.Select(t => CreatePartViewModel(t)));

            Triggers.CollectionChanged += Triggers_CollectionChanged;
            Conditions.CollectionChanged += Conditions_CollectionChanged;
            Actions.CollectionChanged += Actions_CollectionChanged;

            /*
             * 
                    // Clear viewmodels since one may be in use now.
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AllTriggers)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AllConditions)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AllActions)));


    */
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
            PartViewModel ret;
            if (part is EventTrigger)
            {
                ret = new EventTriggerViewModel((EventTrigger)part);
            }
            else if (part is ProcessTrigger)
            {
                ret = new ProcessTriggerViewModel((ProcessTrigger)part);
            }
            else if (part is HotkeyTrigger)
            {
                var hotkeyVm = new HotkeyTriggerViewModel((HotkeyTrigger)part);
                hotkeyVm.SetHotkey = _selectHotkey;
                ret = hotkeyVm;
            }
            else if (part is AudioDeviceEventTrigger)
            {
                ret = new AudioDeviceEventTriggerViewModel((AudioDeviceEventTrigger)part);
            }
            else if (part is AudioDeviceSessionEventTrigger)
            {
                ret = new AudioDeviceSessionEventTriggerViewModel((AudioDeviceSessionEventTrigger)part);
            }
            else if (part is DefaultPlaybackDeviceCondition)
            {
                ret = new DefaultPlaybackDeviceConditionViewModel((DefaultPlaybackDeviceCondition)part);
            }
            else if (part is VariableCondition)
            {
                ret = new VariableConditionViewModel((VariableCondition)part);
            }
            else if (part is ProcessCondition)
            {
                ret = new ProcessConditionViewModel((ProcessCondition)part);
            }
            else if (part is SetVariableAction)
            {
                ret = new SetVariableActionViewModel((SetVariableAction)part);
            }
            else if (part is SetDefaultDeviceAction)
            {
                ret = new SetDefaultDeviceActionViewModel((SetDefaultDeviceAction)part);
            }
            else if (part is ChangeDeviceVolumeAction)
            {
                ret = new ChangeDeviceVolumeActionViewModel((ChangeDeviceVolumeAction)part);
            }
            else if (part is ChangeAppVolumeAction)
            {
                ret = new ChangeAppVolumeActionViewModel((ChangeAppVolumeAction)part);
            }
            else if (part is SetThemeAction)
            {
                ret = new SetThemeActionViewModel((SetThemeAction)part);
            }
            else if (part is SetAddonEarTrumpetSettingsAction)
            {
                ret = new SetAddonEarTrumpetSettingsActionViewModel((SetAddonEarTrumpetSettingsAction)part);
            }
            else
            {
                throw new NotImplementedException();
            }

            ret.Open = new RelayCommand<PartViewModel>((vm) => OpenDialog.Execute(vm));

            return ret;
        }
    }
}
