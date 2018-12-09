using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using EarTrumpet_Actions.DataModel;
using EarTrumpet_Actions.DataModel.Actions;
using EarTrumpet_Actions.DataModel.Conditions;
using EarTrumpet_Actions.DataModel.Triggers;
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

        public List<ContextMenuItem> NewTriggers => PartViewModelFactory.Create<BaseTrigger>().Select(t => MakeItem(t)).ToList();
        public List<ContextMenuItem> NewConditions => PartViewModelFactory.Create<BaseCondition>().Select(t => MakeItem(t)).ToList();
        public List<ContextMenuItem> NewActions => PartViewModelFactory.Create<BaseAction>().Select(t => MakeItem(t)).ToList();

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

        public EarTrumpetAction GetAction()
        {
            _action.DisplayName = DisplayName;
            _action.Triggers = new ObservableCollection<BaseTrigger>(Triggers.Select(t => (BaseTrigger)t.Part));
            _action.Conditions = new ObservableCollection<BaseCondition>(Conditions.Select(t => (BaseCondition)t.Part));
            _action.Actions = new ObservableCollection<BaseAction>(Actions.Select(t => (BaseAction)t.Part));
            return _action;
        }

        private void Parts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var col = (ObservableCollection<PartViewModel>)sender;
            
            for (var i = 0; i < col.Count; i++)
            {
                col[i].IsShowingAdditionalText = i != 0;
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

                    if (part.Part is BaseTrigger)
                    {
                        Triggers.Add(part);
                    }
                    else if (part.Part is BaseCondition)
                    {
                        Conditions.Add(part);
                    }
                    else if (part.Part is BaseAction)
                    {
                        Actions.Add(part);
                    }
                }),
            };
        }

        private PartViewModel CreatePartViewModel(Part part)
        {
            var ret = PartViewModelFactory.From(part);
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
                else if (part.Part is BaseAction)
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
