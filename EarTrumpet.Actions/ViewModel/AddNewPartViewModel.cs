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
                var hotkeyTriggerViewModel = new HotkeyTriggerViewModel(new HotkeyTrigger { });
                hotkeyTriggerViewModel.SetHotkey = SetHotkey;

                return new List<PartViewModel>
                {
                    new EventTriggerViewModel(new EventTrigger{}),
                    new DeviceEventTriggerViewModel(new DeviceEventTrigger{ }),
                    new AppEventTriggerViewModel(new AppEventTrigger{ }),
                    new ProcessTriggerViewModel(new ProcessTrigger{ }),
                    hotkeyTriggerViewModel,
                };
            }
        }

        public List<PartViewModel> Conditions
        {
            get
            {
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
                var ret = new List<PartViewModel>
                {
                    new SetAppVolumeActionViewModel(new SetAppVolumeAction{ }),
                    new SetDeviceVolumeActionViewModel(new SetDeviceVolumeAction{ }),
                    new SetDefaultDeviceActionViewModel(new SetDefaultDeviceAction{ }),
                    new SetVariableActionViewModel(new SetVariableAction{ }),
                };

                var addonValues = ServiceBus.GetMany(KnownServices.ValueService);
                if (addonValues != null && addonValues.Any())
                {
                    ret.Add(new SetAddonEarTrumpetSettingsActionViewModel(new SetAdditionalSettingsAction()));
                }

                if (ServiceBus.Get("EarTrumpet-Themes") != null)
                {
                    ret.Add(new SetThemeActionViewModel(new SetThemeAction { }));
                }
                return ret;
            }
        }

        public AddNewPartViewModel()
        {
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
