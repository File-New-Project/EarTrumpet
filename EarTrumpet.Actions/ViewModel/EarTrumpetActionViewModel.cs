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
        public class SplitListComparer : IComparer<PartViewModel>
        {
            private int GetCost(PartViewModel part)
            {
                if (part.Part is BaseTrigger)
                {
                    return 2;
                }
                else if (part.Part is BaseCondition)
                {
                    return 1;
                }
                return 0;
            }

            public int Compare(PartViewModel one, PartViewModel two)
            {
                var oneCost = GetCost(one);
                var twoCost = GetCost(two);
                if (oneCost != twoCost)
                {
                    return twoCost - oneCost;
                }

                return string.Compare(one.Description, two.Description, StringComparison.Ordinal);
            }
        }

        private static SplitListComparer s_PartComparer = new SplitListComparer();

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

        private PartViewModel _selected;
        public PartViewModel Selected
        {
            get => _selected;
            set
            {
                if (_selected != value)
                {
                    if (_selected != null)
                    {
                        _selected.IsExpanded = false;
                    }
                    _selected = value;
                    if (_selected != null)
                    {
                        _selected.IsExpanded = true;
                    }
                    RaisePropertyChanged(nameof(Selected));
                }
            }
        }


        public ObservableCollection<PartViewModel> Parts { get; }

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

                Parts.AddSorted(vm.SelectedPart, s_PartComparer);

                InitializeViewModel(vm.SelectedPart);
                Selected = vm.SelectedPart;
            });

            Parts = new ObservableCollection<PartViewModel>();
            foreach(var part in action.Triggers.Select(t => CreatePartViewModel(t)))
            {
                Parts.AddSorted(part, s_PartComparer);
            }
            foreach (var part in action.Conditions.Select(t => CreatePartViewModel(t)))
            {
                Parts.AddSorted(part, s_PartComparer);
            }
            foreach (var part in action.Actions.Select(t => CreatePartViewModel(t)))
            {
                Parts.AddSorted(part, s_PartComparer);
            }
        }

        public EarTrumpetAction GetAction()
        {
            _action.DisplayName = DisplayName;
            _action.Triggers = new ObservableCollection<BaseTrigger>(Parts.Where(t => t.Part is BaseTrigger).Select(t => (BaseTrigger)t.Part));
            _action.Conditions = new ObservableCollection<BaseCondition>(Parts.Where(t => t.Part is BaseCondition).Select(t => (BaseCondition)t.Part));
            _action.Actions = new ObservableCollection<BaseAction>(Parts.Where(t => t.Part is BaseAction).Select(t => (BaseAction)t.Part));
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
            part.Remove = new RelayCommand<PartViewModel>((p) => Parts.Remove(p));
            part.Open = new RelayCommand<PartViewModel>((vm) => OpenDialog.Execute(vm));
        }
    }
}
