using EarTrumpet.DataModel.Storage;
using EarTrumpet.UI.Helpers;
using EarTrumpet_Actions.DataModel;
using EarTrumpet_Actions.DataModel.Actions;
using EarTrumpet_Actions.DataModel.Conditions;
using EarTrumpet_Actions.DataModel.Triggers;
using EarTrumpet_Actions.ViewModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace EarTrumpet_Actions.ViewModel
{
    class ActionsEditorViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

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

        public List<PartViewModel> AllTriggers
        {
            get
            {
                return new List<PartViewModel>
                {
                    new PartViewModel(new EventTrigger{}),
                    new PartViewModel(new HotkeyTrigger{  }),
                    new PartViewModel(new AudioDeviceEventTrigger{ }),
                    new PartViewModel(new AudioDeviceSessionEventTrigger{  }),
                    new PartViewModel(new ProcessTrigger{ }),
                };
            }
        }

        public List<PartViewModel> AllConditions
        {
            get
            {
                return new List<PartViewModel>
                {
                    new PartViewModel(new DefaultPlaybackDeviceCondition{  }),
                    new PartViewModel(new ProcessCondition{  }),
                };
            }
        }

        public List<PartViewModel> AllActions
        {
            get
            {
                return new List<PartViewModel>
                {
                    new PartViewModel(new ChangeAppVolumeAction{}),
                    new PartViewModel(new ChangeDeviceVolumeAction{ }),
                    new PartViewModel(new SetDefaultDeviceAction{ }),
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
            EarTrumpetActions = new ObservableCollection<EarTrumpetActionViewModel>(ActionsManager.Instance.Actions.Select(a => new EarTrumpetActionViewModel(a)));

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
                SelectedPart.IsExpanded = false;
                SelectedPart = null;
            });
        }
    }
}