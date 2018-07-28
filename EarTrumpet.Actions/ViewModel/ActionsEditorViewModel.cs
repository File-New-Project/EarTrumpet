using EarTrumpet.DataModel.Storage;
using EarTrumpet.UI.Helpers;
using EarTrumpet_Actions.DataModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace EarTrumpet_Actions.ViewModel
{
    public class ActionsEditorViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Title => "Actions & hotkeys";

        public EarTrumpetActionViewModel SelectedAction
        {
            get => _selectedAction;
            set
            {
                if (value != _selectedAction)
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
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedAction)));
                }
            }
        }
        
        public ObservableCollection<EarTrumpetActionViewModel> Actions { get; }

        public ICommand New { get; }
        public ICommand RemoveItem { get; }
        public ICommand OpenItem { get; }
        public ICommand OpenDialog { get; set; }

        private EarTrumpetActionViewModel _selectedAction;
        private ISettingsBag _settings = StorageFactory.GetSettings("Eartrumpet.Actions");

        public ActionsEditorViewModel()
        {
            New = new RelayCommand(() =>
            {
                var vm = new EarTrumpetActionViewModel(this, new EarTrumpetAction { DisplayName = "New Action" });
                vm.Remove = RemoveItem;
                vm.Open = OpenItem;
                vm.OpenDialog = OpenDialog;
                Actions.Add(vm);
                SelectedAction = vm;
            });

            RemoveItem = new RelayCommand(() =>
            {
                Actions.Remove(SelectedAction);
                SelectedAction = null;
            });

            OpenItem = new RelayCommand(() =>
            {
                OpenDialog.Execute(SelectedAction);
            });

            Actions = new ObservableCollection<EarTrumpetActionViewModel>(Addon.Current.Manager.Actions.Select(a => new EarTrumpetActionViewModel(this, a)));
            foreach (var action in Actions)
            {
                action.Remove = RemoveItem;
                action.Open = OpenItem;
                action.OpenDialog = new RelayCommand<object>((o) => OpenDialog.Execute(o));
            }
        }
    }
}