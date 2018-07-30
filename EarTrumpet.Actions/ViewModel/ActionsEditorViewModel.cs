using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using EarTrumpet_Actions.DataModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace EarTrumpet_Actions.ViewModel
{
    public class ActionsEditorViewModel : BindableBase, IWindowHostedViewModel
    {
#pragma warning disable CS0067
        public event Action Close;
#pragma warning restore CS0067
        public event Action<object> HostDialog;

        public string Title => Properties.Resources.EditActionsAndHotkeysText;

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
                    RaisePropertyChanged(nameof(SelectedAction));
                }
            }
        }
        
        public ObservableCollection<EarTrumpetActionViewModel> Actions { get; }
        public ICommand New { get; }
        public ICommand RemoveItem { get; }
        public ICommand OpenItem { get; }

        private EarTrumpetActionViewModel _selectedAction;
        private ICommand _openDialog;

        public ActionsEditorViewModel(EarTrumpetAction[] actions)
        {
            New = new RelayCommand(() =>
            {
                var vm = new EarTrumpetActionViewModel(this, new EarTrumpetAction { DisplayName = "New Action" });
                vm.Remove = RemoveItem;
                vm.Open = OpenItem;
                vm.OpenDialog = _openDialog;
                Actions.Add(vm);
                SelectedAction = vm;
                _openDialog.Execute(SelectedAction);
            });

            RemoveItem = new RelayCommand(() =>
            {
                Actions.Remove(SelectedAction);
                SelectedAction = null;
            });

            OpenItem = new RelayCommand(() =>
            {
                _openDialog.Execute(SelectedAction);
            });

            _openDialog = new RelayCommand<object>((o) =>
            {
                HostDialog(o);
            });

            Actions = new ObservableCollection<EarTrumpetActionViewModel>(actions.Select(a => new EarTrumpetActionViewModel(this, a)));
            foreach (var action in Actions)
            {
                action.Remove = RemoveItem;
                action.Open = OpenItem;
                action.OpenDialog = new RelayCommand<object>((o) => _openDialog.Execute(o));
            }
        }

        public void OnClosing()
        {

        }

        public void OnPreviewKeyDown(KeyEventArgs e)
        {

        }
    }
}