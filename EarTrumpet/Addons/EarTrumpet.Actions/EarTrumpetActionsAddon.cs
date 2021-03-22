using EarTrumpet.Actions.DataModel;
using EarTrumpet.Actions.DataModel.Processing;
using EarTrumpet.Actions.DataModel.Serialization;
using EarTrumpet.Actions.ViewModel;
using EarTrumpet.DataModel.Storage;
using EarTrumpet.Extensibility;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

namespace EarTrumpet.Actions
{
    [Export(typeof(EarTrumpetAddon))]
    public class EarTrumpetActionsAddon : EarTrumpetAddon, IEarTrumpetAddonEvents, IEarTrumpetAddonSettingsPage, IEarTrumpetAddonNotificationAreaContextMenu
    {
        public static EarTrumpetActionsAddon Current { get; private set; }
        public LocalVariablesContainer LocalVariables { get; private set; }

        public EarTrumpetActionsAddon() : base()
        {
            DisplayName = Properties.Resources.MyActionsText;
        }

        public EarTrumpetAction[] Actions
        {
            get => _actions;
            set
            {
                Settings.Set(c_actionsSettingKey, value);
                LoadAndRegister();
            }
        }

        private readonly string c_actionsSettingKey = "ActionsData";
        private EarTrumpetAction[] _actions = new EarTrumpetAction[] { };
        private TriggerManager _triggerManager = new TriggerManager();

        public void OnAddonEvent(AddonEventKind evt)
        {
            if (evt == AddonEventKind.AddonsInitialized)
            {
                Current = this;
                LocalVariables = new LocalVariablesContainer(Settings);

                _triggerManager.Triggered += OnTriggered;
                LoadAndRegister();

                _triggerManager.OnEvent(AddonEventKind.InitializeAddon);
            }
            else if (evt == AddonEventKind.AppShuttingDown)
            {
                _triggerManager.OnEvent(AddonEventKind.AppShuttingDown);
            }
        }

        public SettingsCategoryViewModel GetSettingsCategory()
        {
            LoadAddonResources();
            return new ActionsCategoryViewModel();
        }

        public IEnumerable<ContextMenuItem> NotificationAreaContextMenuItems
        {
            get
            {
                var ret = new List<ContextMenuItem>();

                if (EarTrumpetActionsAddon.Current == null)
                {
                    return ret;
                }

                foreach (var item in EarTrumpetActionsAddon.Current.Actions.Where(a => a.Triggers.FirstOrDefault(ax => ax is ContextMenuTrigger) != null))
                {
                    ret.Add(new ContextMenuItem
                    {
                        Glyph = "\xE1CE",
                        IsChecked = true,
                        DisplayName = item.DisplayName,
                        Command = new RelayCommand(() => EarTrumpetActionsAddon.Current.TriggerAction(item))
                    });
                }
                return ret;
            }
        }

        private void LoadAndRegister()
        {
            _triggerManager.Clear();
            _actions = Settings.Get(c_actionsSettingKey, new EarTrumpetAction[] { });
            _actions.SelectMany(a => a.Triggers).ToList().ForEach(t => _triggerManager.Register(t));
        }

        public void Import(string fileName)
        {
            var imported = Serializer.FromString<EarTrumpetAction[]>(File.ReadAllText(fileName)).ToList();
            foreach(var imp in imported)
            {
                imp.Id = Guid.NewGuid();
            }
            imported.AddRange(Actions);
            Actions = imported.ToArray();
        }

        public string Export()
        {
            return Settings.Get(c_actionsSettingKey, "");
        }

        private void OnTriggered(BaseTrigger trigger)
        {
            var action = Actions.FirstOrDefault(a => a.Triggers.Contains(trigger));
            if (action != null && action.Conditions.All(c => ConditionProcessor.IsMet(c)))
            {
                TriggerAction(action);
            }
        }

        public void TriggerAction(EarTrumpetAction action)
        {
            action.Actions.ToList().ForEach(a => ActionProcessor.Invoke(a));
        }
    }
}