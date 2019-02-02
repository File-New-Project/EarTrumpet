using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using EarTrumpet_Actions.DataModel;
using EarTrumpet_Actions.DataModel.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace EarTrumpet_Actions.ViewModel
{
    public class EarTrumpetActionViewModel : SettingsPageViewModel
    {
        public ToolbarItemViewModel[] Toolbar { get; private set; }

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
        private ActionsCategoryViewModel _parent;

        public EarTrumpetActionViewModel(ActionsCategoryViewModel parent, EarTrumpetAction action) : base("Saved Actions")
        {
            _parent = parent;
            _action = action;
            DisplayName = _action.DisplayName;
            Title = DisplayName;
            Glyph = "\xE1CE";

            Triggers = new ObservableCollection<PartViewModel>(action.Triggers.Select(t => CreatePartViewModel(t)));
            Conditions = new ObservableCollection<PartViewModel>(action.Conditions.Select(t => CreatePartViewModel(t)));
            Actions = new ObservableCollection<PartViewModel>(action.Actions.Select(t => CreatePartViewModel(t)));

            Triggers.CollectionChanged += Parts_CollectionChanged;
            Conditions.CollectionChanged += Parts_CollectionChanged;
            Actions.CollectionChanged += Parts_CollectionChanged;

            Parts_CollectionChanged(Triggers, null);
            Parts_CollectionChanged(Conditions, null);
            Parts_CollectionChanged(Actions, null);

            Toolbar = new ToolbarItemViewModel[]
            {
                new ToolbarItemViewModel
                {
                     Command = new RelayCommand(() =>
                     {
                         _parent.Save(this);
                     }),
                     DisplayName = "Save",
                     Glyph = "\xE105",
                     GlyphFontSize = 20,
                },
                new ToolbarItemViewModel
                {
                     Command = new RelayCommand(() =>
                     {
                         _parent.Delete(this);
                     }),
                     DisplayName = "Delete",
                     Glyph = "\xE107",
                     GlyphFontSize = 20,
                }
            };
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
