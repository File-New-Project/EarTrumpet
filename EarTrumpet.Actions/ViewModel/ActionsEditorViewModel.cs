using EarTrumpet.DataModel.Storage;
using EarTrumpet.Extensibility.Shared;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.Services;
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
    public class ActionsEditorViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event Action<EarTrumpetAction[]> RequestApplyChanges;
        public event Action<PartViewModel> PartSelected;

        public string Title => "Actions & hotkeys";

#pragma warning disable CS0067
        public event Func<HotkeyData, HotkeyData> RequestHotkey;
#pragma warning restore CS0067

        private EarTrumpetActionViewModel _selectedAction;
        private PartViewModel _selectedPart;
        private ISettingsBag _settings = StorageFactory.GetSettings("Eartrumpet.Actions");

        public EarTrumpetActionViewModel SelectedAction
        {
            get => _selectedAction;
            set
            {
                if (value != _selectedAction)
                {
                    _selectedAction = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedAction)));
                }
            }
        }

        public bool SelectingPart => SelectedPart != null;
        public PartViewModel SelectedPart
        {
            get => _selectedPart;
            set
            {
                if (value != _selectedPart)
                {
                    _selectedPart = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedPart)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectingPart)));

                    // Clear viewmodels since one may be in use now.
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AllTriggers)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AllConditions)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AllActions)));

                    if (SelectedPart != null)
                    {
                        PartSelected?.Invoke(SelectedPart);
                    }
                }
            }
        }

        public ObservableCollection<EarTrumpetActionViewModel> EarTrumpetActions { get; }

        public ICommand NewEarTrumpetAction { get; }
        public ICommand Save { get; }
        public ICommand UnselectAction { get; }
        public ICommand UnselectPart { get; }
        public ICommand DeleteAction { get; }

        public List<object> AllTriggers
        {
            get
            {
                return new List<object>
                {
                    new EventTriggerViewModel(new EventTrigger{}),
                    new AudioDeviceEventTriggerViewModel(new AudioDeviceEventTrigger{ }),                    
                    new AudioDeviceSessionEventTriggerViewModel(new AudioDeviceSessionEventTrigger{ }),     
                    new ProcessTriggerViewModel(new ProcessTrigger{ }),
                    new HotkeyTriggerViewModel(this, new HotkeyTrigger{ }),
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

                if (ServiceBus.Exists("EarTrumpet-Themes"))
                {
                    ret.Add(new SetThemeActionViewModel(new SetThemeAction { }));
                }
                return ret;
            }
        }

        public ActionsEditorViewModel()
        {
            EarTrumpetActions = new ObservableCollection<EarTrumpetActionViewModel>(Addon.Current.Manager.Actions.Select(a => new EarTrumpetActionViewModel(this, a)));

            NewEarTrumpetAction = new RelayCommand(() =>
            {
                var vm = new EarTrumpetActionViewModel(this, new EarTrumpetAction { DisplayName = "New Action" });
                EarTrumpetActions.Add(vm);
                SelectedAction = vm;
            });

            Save = new RelayCommand(() =>
            {
                RequestApplyChanges?.Invoke(EarTrumpetActions.Select(a => a.GetAction()).ToArray());
            });
            UnselectAction = new RelayCommand(() =>
            {
                SelectedAction = null;
            });
            DeleteAction = new RelayCommand(() =>
            {
                EarTrumpetActions.Remove(SelectedAction);
                SelectedAction = null;
            });
        }

        internal HotkeyData GetHotkey(HotkeyData hotkey)
        {
            return RequestHotkey(hotkey);
        }
    }
}