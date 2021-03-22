using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using EarTrumpet.Actions.DataModel;
using EarTrumpet.Actions.DataModel.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace EarTrumpet.Actions.ViewModel
{
    public class EarTrumpetActionViewModel : SettingsPageViewModel
    {
        public ToolbarItemViewModel[] Toolbar { get; private set; }
        public ICommand Delete => new RelayCommand(() => _parent.Delete(this));
        public Guid Id => _action.Id;

        public string DisplayName
        {
            get => _action.DisplayName;
            set
            {
                if (DisplayName != value)
                {
                    _action.DisplayName = value;
                    RaisePropertyChanged(nameof(DisplayName));
                    Title = DisplayName;

                    IsWorkSaved = false;
                    IsPersisted = true;
                }
            }
        }

        private bool _isEditClicked;
        public bool IsEditClicked
        {
            get => _isEditClicked;
            set
            {
                if (_isEditClicked != value)
                {
                    _isEditClicked = value;
                    RaisePropertyChanged(nameof(IsEditClicked));

                    // Immediately unset the value so we can go again.
                    _isEditClicked = false;
                    RaisePropertyChanged(nameof(IsEditClicked));
                }
            }
        }

        private bool _isWorkSaved;
        public bool IsWorkSaved
        {
            get => _isWorkSaved;
            set
            {
                if (_isWorkSaved != value)
                {
                    _isWorkSaved = value;
                    RaisePropertyChanged(nameof(IsWorkSaved));
                }
            }
        }

        public List<ContextMenuItem> NewTriggers => PartViewModelFactory.Create<BaseTrigger>().Select(t => MakeItem(t)).OrderBy(t => t.DisplayName).ToList();
        public List<ContextMenuItem> NewConditions => PartViewModelFactory.Create<BaseCondition>().Select(t => MakeItem(t)).OrderBy(t => t.DisplayName).ToList();
        public List<ContextMenuItem> NewActions => PartViewModelFactory.Create<BaseAction>().Select(t => MakeItem(t)).OrderBy(t => t.DisplayName).ToList();

        public ObservableCollection<PartViewModel> Triggers { get; private set; }
        public ObservableCollection<PartViewModel> Conditions { get; private set; }
        public ObservableCollection<PartViewModel> Actions { get; private set; }
        public bool IsPersisted { get; set; } = true;

        private EarTrumpetAction _action;
        private ActionsCategoryViewModel _parent;

        public EarTrumpetActionViewModel(ActionsCategoryViewModel parent, EarTrumpetAction action) : base("Saved Actions")
        {
            _parent = parent;
            Reset(action);
            Header = new EarTrumpetActionPageHeaderViewModel(this);
            Toolbar = new ToolbarItemViewModel[]
            {
                new ToolbarItemViewModel
                {
                     Command = new RelayCommand(() =>
                     {
                         IsEditClicked = true;
                     }),
                     DisplayName = Properties.Resources.ToolbarEditText,
                     Glyph = "\xE70F",
                     GlyphFontSize = 15,
                },
                new ToolbarItemViewModel
                {
                     Command = new RelayCommand(() =>
                     {
                         IsPersisted = true;
                         _parent.Save(this);
                     }),
                     DisplayName = Properties.Resources.ToolbarSaveText,
                     Id = "Save",
                     Glyph = "\xE105",
                     GlyphFontSize = 15,
                },
            };

            Glyph = "\xE1CE";
            Title = DisplayName;
        }

        public void Reset(EarTrumpetAction action)
        {
            _action = action;

            Title = DisplayName;
            Triggers = new ObservableCollection<PartViewModel>(action.Triggers.Select(t => CreatePartViewModel(t)));
            Conditions = new ObservableCollection<PartViewModel>(action.Conditions.Select(t => CreatePartViewModel(t)));
            Actions = new ObservableCollection<PartViewModel>(action.Actions.Select(t => CreatePartViewModel(t)));

            Triggers.CollectionChanged += Parts_CollectionChanged;
            Conditions.CollectionChanged += Parts_CollectionChanged;
            Actions.CollectionChanged += Parts_CollectionChanged;

            Parts_CollectionChanged(Triggers, null);
            Parts_CollectionChanged(Conditions, null);
            Parts_CollectionChanged(Actions, null);
            RaisePropertyChanged(nameof(Triggers));
            RaisePropertyChanged(nameof(Conditions));
            RaisePropertyChanged(nameof(Actions));
            RaisePropertyChanged(nameof(DisplayName));
            IsWorkSaved = true;
        }


        public override bool NavigatingFrom(NavigationCookie cookie)
        {
            if (!IsWorkSaved && IsPersisted)
            {
                _parent.ShowDialog(Properties.Resources.LeavingPageDialogTitle, Properties.Resources.LeavingPageDialogText, Properties.Resources.LeavingPageDialogYesText, () =>
                {
                    _parent.CompleteNavigation(cookie);

                    var existing = EarTrumpetActionsAddon.Current.Actions.FirstOrDefault(a => a.Id == Id);
                    if (existing == null)
                    {
                        _parent.Delete(this, true);
                    }
                    else
                    {
                        Reset(existing);
                    }
                    
                }, Properties.Resources.LeavingPageDialogNoText, () => { });
                return false;
            }
            return base.NavigatingFrom(cookie);
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
            IsWorkSaved = false;
            IsPersisted = true;
        }

        private ContextMenuItem MakeItem(PartViewModel part)
        {
            return new ContextMenuItem
            {
                DisplayName = part.AddText,
                Command = new RelayCommand(() =>
                {
                    InitializeViewModel(part);
                    GetListFromPart(part).Add(part);
                }),
            };
        }

        private PartViewModel CreatePartViewModel(Part part)
        {
            var ret = PartViewModelFactory.Create(part);
            InitializeViewModel(ret);
            return ret;
        }

        private void InitializeViewModel(PartViewModel part)
        {
            part.PropertyChanged += (_, __) => IsWorkSaved = false;
            part.Remove = new RelayCommand(() => GetListFromPart(part).Remove(part));
        }

        private ObservableCollection<PartViewModel> GetListFromPart(PartViewModel part)
        {
            if (part.Part is BaseTrigger)
            {
                return Triggers;
            }
            else if (part.Part is BaseCondition)
            {
                return Conditions;
            }
            else if (part.Part is BaseAction)
            {
                return Actions;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
