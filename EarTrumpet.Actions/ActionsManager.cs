using EarTrumpet.DataModel.Storage;
using EarTrumpet.Extensibility;
using EarTrumpet_Actions.DataModel;
using EarTrumpet_Actions.DataModel.Triggers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace EarTrumpet_Actions
{
    public class ActionsManager
    {
        public ObservableCollection<EarTrumpetAction> Actions { get; private set; }
        public Dictionary<string, bool> LocalVariables { get; }

        private ISettingsBag _settings = StorageFactory.GetSettings("Eartrumpet.Actions");
        private TriggerManager _triggerManager = new TriggerManager();

        public ActionsManager()
        {
            LocalVariables = new Dictionary<string, bool>();
            PlaybackDataModelHost.InitializeDataModel();
            _triggerManager.Triggered += OnTriggered;
        }

        public void OnStartup()
        {
            LoadFromDisk();

            _triggerManager.OnEvent(ApplicationLifecycleEvent.Startup);
        }

        private void LoadFromDisk()
        {
            try
            {
                var data = _settings.Get("ActionsData", new EarTrumpetAction[] { });
                Actions = new ObservableCollection<EarTrumpetAction>(data);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Failure loading settings: {ex}");
                Actions = new ObservableCollection<EarTrumpetAction>();
            }

            foreach (var t in Actions.SelectMany(a => a.Triggers))
            {
                _triggerManager.Register(t);
            }
        }

        public void Apply(EarTrumpetAction[] newActions)
        {
            _settings.Set("ActionsData", newActions);

            Actions = new ObservableCollection<EarTrumpetAction>(newActions);

            foreach (var t in Actions.SelectMany(a => a.Triggers))
            {
                _triggerManager.Register(t);
            }
        }

        private void OnTriggered(BaseTrigger trigger)
        {
            var action = Actions.FirstOrDefault(a => a.Triggers.Contains(trigger));

            if (action != null && action.Conditions.All(c => ConditionProcessor.IsMet(c)))
            {
                action.Actions.ToList().ForEach(a => ActionProcessor.Invoke(a));
            }
        }

        public void OnShuttingDown()
        {
            _triggerManager.OnEvent(ApplicationLifecycleEvent.Startup);
        }
    }
}
