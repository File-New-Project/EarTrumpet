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
using System.Collections.ObjectModel;
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
        public ICommand AddPart { get; }

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

            AddPart = new RelayCommand(() =>
            {
                var vm = new AddNewPartViewModel();
                vm.SetHotkey = _selectHotkey;
                OpenDialog.Execute(vm);

                if (vm.SelectedPart == null)
                {
                    return;
                }

                if (vm.SelectedPart.Part is BaseTrigger)
                {
                    Triggers.Add(vm.SelectedPart);
                }
                else if (vm.SelectedPart.Part is BaseCondition)
                {
                    Conditions.Add(vm.SelectedPart);
                }
                else if (vm.SelectedPart.Part is BaseAction)
                {
                    Actions.Add(vm.SelectedPart);
                }

                InitializeViewModel(vm.SelectedPart);
            });

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

            InitializeViewModel(ret);

            return ret;
        }

        private void InitializeViewModel(PartViewModel part)
        {
            var removeTrigger = new RelayCommand<PartViewModel>((p) => Triggers.Remove(p));
            var removeCondition = new RelayCommand<PartViewModel>((p) => Conditions.Remove(p));
            var removeAction = new RelayCommand<PartViewModel>((p) => Actions.Remove(p));

            part.Open = new RelayCommand<PartViewModel>((vm) => OpenDialog.Execute(vm));

            if (part.Part is BaseTrigger)
            {
                part.Remove = removeTrigger;
            }
            else if (part.Part is BaseCondition)
            {
                part.Remove = removeCondition;
            }
            else if (part.Part is BaseAction)
            {
                part.Remove = removeAction;
            }
        }
    }
}
