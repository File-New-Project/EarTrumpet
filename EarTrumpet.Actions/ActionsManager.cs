using EarTrumpet.DataModel.Storage;
using EarTrumpet.Extensibility;
using EarTrumpet_Actions.DataModel;
using EarTrumpet_Actions.DataModel.Triggers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            Load(_settings.Get("ActionsData", new EarTrumpetAction[] { }));

            _triggerManager.OnEvent(ApplicationLifecycleEvent.Startup);
        }

        public void Apply(EarTrumpetAction[] newActions)
        {
            _settings.Set("ActionsData", newActions);

            Load(newActions);
        }

        public void Load(EarTrumpetAction[] newActions)
        {
            Actions = new ObservableCollection<EarTrumpetAction>(newActions);
            Actions.SelectMany(a => a.Triggers).ToList().ForEach(t => _triggerManager.Register(t));
        }

        private void OnTriggered(BaseTrigger trigger)
        {
            var action = Actions.FirstOrDefault(a => a.Triggers.Contains(trigger));

            if (action != null && action.Conditions.All(c => ConditionProcessor.IsMet(c)))
            {
                action.Actions.ToList().ForEach(a => ActionProcessor.Invoke(a));
            }
        }

        public void OnShuttingDown() => _triggerManager.OnEvent(ApplicationLifecycleEvent.Shutdown);
    }
}
