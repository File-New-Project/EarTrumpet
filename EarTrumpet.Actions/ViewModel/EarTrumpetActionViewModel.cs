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
using System.Linq;
using System.Windows.Input;

namespace EarTrumpet_Actions.ViewModel
{
    public class EarTrumpetActionViewModel : BindableBase, IWindowHostedViewModel, IWindowHostedViewModelInternal
    {
        public string Title => DisplayName;

        public ICommand Open { get; set; }
        public ICommand Save { get; set; }
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

        public List<ContextMenuItem> NewTriggers
        {
            get
            {
                return new List<ContextMenuItem>
                {
                    MakeItem(new EventTriggerViewModel(new EventTrigger{})),
                    MakeItem(new DeviceEventTriggerViewModel(new DeviceEventTrigger{ })),
                    MakeItem(new AppEventTriggerViewModel(new AppEventTrigger{ })),
                    MakeItem(new ProcessTriggerViewModel(new ProcessTrigger{ })),
                    MakeItem(new ContextMenuTriggerViewModel(new ContextMenuTrigger{ })),
                    MakeItem(new HotkeyTriggerViewModel(new HotkeyTrigger { })),
                };
            }
        }

        public List<ContextMenuItem> NewConditions
        {
            get
            {
                return new List<ContextMenuItem>
                {
                    MakeItem(new DefaultDeviceConditionViewModel(new DefaultDeviceCondition{ })),
                    MakeItem(new ProcessConditionViewModel(new ProcessCondition{ })),
                    MakeItem(new VariableConditionViewModel(new VariableCondition{ })),
                };
            }
        }

        public List<ContextMenuItem> NewActions
        {
            get
            {
                var ret = new List<ContextMenuItem>
                {
                    MakeItem(new SetAppVolumeActionViewModel(new SetAppVolumeAction{ })),
                    MakeItem(new SetAppMuteActionViewModel(new SetAppMuteAction{ })),
                    MakeItem(new SetDeviceVolumeActionViewModel(new SetDeviceVolumeAction{ })),
                    MakeItem(new SetDeviceMuteActionViewModel(new SetDeviceMuteAction{ })),
                    MakeItem(new SetDefaultDeviceActionViewModel(new SetDefaultDeviceAction{ })),
                    MakeItem(new SetVariableActionViewModel(new SetVariableAction{ })),
                };

                var addonValues = ServiceBus.GetMany(KnownServices.BoolValue);
                if (addonValues.Any())
                {
                    ret.Add(MakeItem(new SetAddonEarTrumpetSettingsActionViewModel(new SetAdditionalSettingsAction())));
                }

                var addonCommands = ServiceBus.GetMany(KnownServices.Command);
                if (addonCommands.Any())
                {
                    ret.Add(MakeItem(new InvokeAddonCommandActionViewModel(new InvokeAddonCommandAction())));
                }

                if (ServiceBus.Get("EarTrumpet-Themes") != null)
                {
                    ret.Add(MakeItem(new SetThemeActionViewModel(new SetThemeAction { })));
                }
                return ret;
            }
        }

        private ContextMenuItem MakeItem(PartViewModel part)
        {
            return new ContextMenuItem
            {
                DisplayName = part.Description,
                Command = new RelayCommand(() =>
                {
                    InitializeViewModel(part);

                    if (part.Part is BaseTrigger) Triggers.Add(part);
                    else if (part.Part is BaseCondition) Conditions.Add(part);
                    else if (part.Part is BaseAction) Actions.Add(part);
                }),
            };
        }

        public ObservableCollection<PartViewModel> Triggers { get; }
        public ObservableCollection<PartViewModel> Conditions { get; }
        public ObservableCollection<PartViewModel> Actions { get; }

        private readonly EarTrumpetAction _action;
        private ActionsEditorViewModel _parent;

#pragma warning disable CS0067
        public event Action Close;
        public event Action<object> HostDialog;
#pragma warning restore CS0067

        public EarTrumpetActionViewModel(ActionsEditorViewModel parent, EarTrumpetAction action)
        {
            _parent = parent;
            _action = action;
            DisplayName = _action.DisplayName;

            Triggers = new ObservableCollection<PartViewModel>(action.Triggers.Select(t => CreatePartViewModel(t)));
            Conditions = new ObservableCollection<PartViewModel>(action.Conditions.Select(t => CreatePartViewModel(t)));
            Actions = new ObservableCollection<PartViewModel>(action.Actions.Select(t => CreatePartViewModel(t)));

            Triggers.CollectionChanged += Parts_CollectionChanged;
            Conditions.CollectionChanged += Parts_CollectionChanged;
            Actions.CollectionChanged += Parts_CollectionChanged;

            Parts_CollectionChanged(Triggers, null);
            Parts_CollectionChanged(Conditions, null);
            Parts_CollectionChanged(Actions, null);
        }

        private void Parts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var col = (ObservableCollection<PartViewModel>)sender;
            
            for (var i = 0; i < col.Count; i++)
            {
                col[i].IsShowingAdditionalText = i != 0;
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
            else if (part is ContextMenuTrigger)
            {
                ret = new ContextMenuTriggerViewModel((ContextMenuTrigger)part);
            }
            else if (part is HotkeyTrigger)
            {
                ret = new HotkeyTriggerViewModel((HotkeyTrigger)part);
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
            else if (part is SetDeviceMuteAction)
            {
                ret = new SetDeviceMuteActionViewModel((SetDeviceMuteAction)part);
            }
            else if (part is SetAppVolumeAction)
            {
                ret = new SetAppVolumeActionViewModel((SetAppVolumeAction)part);
            }
            else if (part is SetAppMuteAction)
            {
                ret = new SetAppMuteActionViewModel((SetAppMuteAction)part);
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
            part.Remove = new RelayCommand(() =>
            {
                if (part.Part is BaseTrigger)
                {
                    Triggers.Remove(part);
                }
                else if (part.Part is BaseCondition)
                {
                    Conditions.Remove(part);
                }
                if (part.Part is BaseAction)
                {
                    Actions.Remove(part);
                }
            });
        }

        public void OnClosing()
        {

        }

        public void OnPreviewKeyDown(KeyEventArgs e)
        {

        }

        void IWindowHostedViewModelInternal.HostDialog(object dialog) => HostDialog(dialog);
    }
}
