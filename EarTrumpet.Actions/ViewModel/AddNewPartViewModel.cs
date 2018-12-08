using EarTrumpet.Extensibility;
using EarTrumpet.Extensibility.Shared;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using EarTrumpet_Actions.DataModel.Actions;
using EarTrumpet_Actions.DataModel.Conditions;
using EarTrumpet_Actions.DataModel.Triggers;
using EarTrumpet_Actions.ViewModel.Actions;
using EarTrumpet_Actions.ViewModel.Conditions;
using EarTrumpet_Actions.ViewModel.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace EarTrumpet_Actions.ViewModel
{
    class AddNewPartViewModel : BindableBase, IWindowHostedViewModel
    {
        public enum Mode
        {
            Triggers,
            Conditions,
            Actions
        }

        public event Action Close;
#pragma warning disable CS0067
        public event Action<object> HostDialog;
#pragma warning restore CS0067

        public string Title => Properties.Resources.AddDialogTitleText;
        public ICommand SetHotkey { get; set; }
        public ICommand Select { get; set; }
        public PartViewModel SelectedPart { get; private set; }

        public List<PartViewModel> Triggers
        {
            get
            {
                if (_mode != Mode.Triggers) return null;

                var hotkeyTriggerViewModel = new HotkeyTriggerViewModel(new HotkeyTrigger { });
                hotkeyTriggerViewModel.SetHotkey = SetHotkey;

                return new List<PartViewModel>
                {
                    new EventTriggerViewModel(new EventTrigger{}),
                    new DeviceEventTriggerViewModel(new DeviceEventTrigger{ }),
                    new AppEventTriggerViewModel(new AppEventTrigger{ }),
                    new ProcessTriggerViewModel(new ProcessTrigger{ }),
                    new ContextMenuTriggerViewModel(new ContextMenuTrigger{ }),
                    hotkeyTriggerViewModel,
                };
            }
        }

        public List<PartViewModel> Conditions
        {
            get
            {
                if (_mode != Mode.Conditions) return null;

                return new List<PartViewModel>
                {
                    new DefaultDeviceConditionViewModel(new DefaultDeviceCondition{ }),
                    new ProcessConditionViewModel(new ProcessCondition{ }),
                    new VariableConditionViewModel(new VariableCondition{ }),
                };
            }
        }

        public List<PartViewModel> Actions
        {
            get
            {
                if (_mode != Mode.Actions) return null;

                var ret = new List<PartViewModel>
                {
                    new SetAppVolumeActionViewModel(new SetAppVolumeAction{ }),
                    new SetDeviceVolumeActionViewModel(new SetDeviceVolumeAction{ }),
                    new SetDefaultDeviceActionViewModel(new SetDefaultDeviceAction{ }),
                    new SetVariableActionViewModel(new SetVariableAction{ }),
                };

                var addonValues = ServiceBus.GetMany(KnownServices.BoolValue);
                if (addonValues.Any())
                {
                    ret.Add(new SetAddonEarTrumpetSettingsActionViewModel(new SetAdditionalSettingsAction()));
                }

                var addonCommands = ServiceBus.GetMany(KnownServices.Command);
                if (addonCommands.Any())
                {
                    ret.Add(new InvokeAddonCommandActionViewModel(new InvokeAddonCommandAction()));
                }

                if (ServiceBus.Get("EarTrumpet-Themes") != null)
                {
                    ret.Add(new SetThemeActionViewModel(new SetThemeAction { }));
                }
                return ret;
            }
        }

        private Mode _mode;

        public AddNewPartViewModel(Mode mode)
        {
            _mode = mode;
            Select = new RelayCommand<PartViewModel>((part) =>
            {
                SelectedPart = part;
                Close?.Invoke();
            });
        }

        public void OnClosing()
        {

        }

        public void OnPreviewKeyDown(KeyEventArgs e)
        {

        }
    }
}
