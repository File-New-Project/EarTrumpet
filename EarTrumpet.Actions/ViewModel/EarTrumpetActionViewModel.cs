using EarTrumpet.Extensions;
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
        public ICommand AddTrigger { get; }
        public ICommand AddCondition { get; }
        public ICommand AddAction { get; }

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

        public ObservableCollection<PartViewModel> Triggers { get; }
        public ObservableCollection<PartViewModel> Conditions { get; }
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

            AddTrigger = new RelayCommand(() =>
            {
                var vm = new AddNewPartViewModel(AddNewPartViewModel.Mode.Triggers);
                vm.SetHotkey = _selectHotkey;
                OpenDialog.Execute(vm);

                if (vm.SelectedPart != null)
                {
                    Triggers.Add(vm.SelectedPart);

                    InitializeViewModel(vm.SelectedPart);
                }
            });

            AddCondition = new RelayCommand(() =>
            {
                var vm = new AddNewPartViewModel(AddNewPartViewModel.Mode.Conditions);
                vm.SetHotkey = _selectHotkey;
                OpenDialog.Execute(vm);

                if (vm.SelectedPart != null)
                {
                    Conditions.Add(vm.SelectedPart);

                    InitializeViewModel(vm.SelectedPart);
                }
            });

            AddAction = new RelayCommand(() =>
            {
                var vm = new AddNewPartViewModel(AddNewPartViewModel.Mode.Actions);
                vm.SetHotkey = _selectHotkey;
                OpenDialog.Execute(vm);

                if (vm.SelectedPart != null)
                {
                    Actions.Add(vm.SelectedPart);

                    InitializeViewModel(vm.SelectedPart);
                }
            });

            Triggers = new ObservableCollection<PartViewModel>(action.Triggers.Select(t => CreatePartViewModel(t)));
            Conditions = new ObservableCollection<PartViewModel>(action.Conditions.Select(t => CreatePartViewModel(t)));
            Actions = new ObservableCollection<PartViewModel>(action.Actions.Select(t => CreatePartViewModel(t)));
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
            else if (part is ContextMenuTrigger)
            {
                ret = new ContextMenuTriggerViewModel((ContextMenuTrigger)part);
            }
            else if (part is HotkeyTrigger)
            {
                var hotkeyVm = new HotkeyTriggerViewModel((HotkeyTrigger)part);
                hotkeyVm.SetHotkey = _selectHotkey;
                ret = hotkeyVm;
            }
            else if (part is DeviceEventTrigger)
            {
                ret = new DeviceEventTriggerViewModel((DeviceEventTrigger)part);
            }
            else if (part is AppEventTrigger)
            {
                ret = new AppEventTriggerViewModel((AppEventTrigger)part);
            }
            else if (part is DefaultDeviceCondition)
            {
                ret = new DefaultDeviceConditionViewModel((DefaultDeviceCondition)part);
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
            else if (part is SetDeviceVolumeAction)
            {
                ret = new SetDeviceVolumeActionViewModel((SetDeviceVolumeAction)part);
            }
            else if (part is SetAppVolumeAction)
            {
                ret = new SetAppVolumeActionViewModel((SetAppVolumeAction)part);
            }
            else if (part is SetThemeAction)
            {
                ret = new SetThemeActionViewModel((SetThemeAction)part);
            }
            else if (part is SetAdditionalSettingsAction)
            {
                ret = new SetAddonEarTrumpetSettingsActionViewModel((SetAdditionalSettingsAction)part);
            }
            else if (part is InvokeAddonCommandAction)
            {
                ret = new InvokeAddonCommandActionViewModel((InvokeAddonCommandAction)part);
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
            part.Remove = new RelayCommand<PartViewModel>((p) =>
            {
                if (p.Part is BaseTrigger)
                {
                    Triggers.Remove(p);
                }
                else if (p.Part is BaseCondition)
                {
                    Conditions.Remove(p);
                }
                if (p.Part is BaseAction)
                {
                    Actions.Remove(p);
                }
            });
            part.Open = new RelayCommand<PartViewModel>((vm) => OpenDialog.Execute(vm));
        }
    }
}
