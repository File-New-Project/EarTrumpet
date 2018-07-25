using EarTrumpet.DataModel.Storage;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.Services;
using EarTrumpet.UI.ViewModels;
using EarTrumpet_Actions.DataModel;
using EarTrumpet_Actions.DataModel.Actions;
using EarTrumpet_Actions.DataModel.Conditions;
using EarTrumpet_Actions.DataModel.Triggers;
using EarTrumpet_Actions.ViewModel;
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
    public class ActionsEditorViewModel : INotifyPropertyChanged, ISettingsViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

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
                    new HotkeyTriggerViewModel(new HotkeyTrigger{ }),
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
                return new List<PartViewModel>
                {
                    new ChangeAppVolumeActionViewModel(new ChangeAppVolumeAction{ }),
                    new ChangeDeviceVolumeActionViewModel(new ChangeDeviceVolumeAction{ }),
                    new SetDefaultDeviceActionViewModel(new SetDefaultDeviceAction{ }),
                    new SetVariableActionViewModel(new SetVariableAction{ }),
                };
            }
        }

        public Device[] Devices
        {
            get
            {
                var ret = new List<Device>();
                ret.Add(new Device { Id = null });
                foreach (var d in PlaybackDataModelHost.DeviceManager.Devices)
                {
                    ret.Add(new Device { Id = d.Id });
                }
                return ret.ToArray();
            }
        }

        public App[] Apps
        {
            get
            {
                var ret = new HashSet<App>();
                foreach (var s in PlaybackDataModelHost.DeviceManager.Devices.SelectMany(d => d.Groups))
                {
                    ret.Add(new App { Id = s.Id });
                }
                return ret.ToArray();
            }
        }

        public ActionsEditorViewModel()
        {
            EarTrumpetActions = new ObservableCollection<EarTrumpetActionViewModel>(Addon.Current.Manager.Actions.Select(a => new EarTrumpetActionViewModel(a)));

            NewEarTrumpetAction = new RelayCommand(() =>
            {
                var action = new EarTrumpetAction();
                action.DisplayName = "New Action";
                action.Loaded();
                var vm = new EarTrumpetActionViewModel(action);
                // TODO: name collide 1,2,...
                EarTrumpetActions.Add(vm);
                SelectedAction = vm;
            });

            Save = new RelayCommand(() =>
            {
                _settings.Set("ActionsData", EarTrumpetActions.Select(a => a.GetAction()).ToArray());
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
            UnselectPart = new RelayCommand(() =>
            {
             //   SelectedPart.IsExpanded = false;
                SelectedPart = null;
            });
        }
    }
}