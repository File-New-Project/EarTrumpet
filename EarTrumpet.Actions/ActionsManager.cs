using EarTrumpet.DataModel.Storage;
using EarTrumpet.Extensibility;
using EarTrumpet_Actions.DataModel;
using EarTrumpet_Actions.DataModel.Triggers;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace EarTrumpet_Actions
{
    class ActionsManager
    {
        public static ActionsManager Instance = new ActionsManager();

        public ProcessWatcher ProcessWatcher { get; private set; }

        public ObservableCollection<EarTrumpetAction> Actions { get; private set; }

        private ISettingsBag _settings = StorageFactory.GetSettings("Eartrumpet.Actions");

        public void OnEvent(ApplicationLifecycleEvent evt)
        {
            if (evt == ApplicationLifecycleEvent.Startup)
            {
                OnStartup();
            }
            else if (evt == ApplicationLifecycleEvent.Shutdown)
            {
                OnShuttingDown();
            }
        }

        private void OnStartup()
        {
            PlaybackDataModelHost.InitializeDataModel();

            ProcessWatcher = new ProcessWatcher();

            try
            {
                var data = _settings.Get<EarTrumpetAction[]>("ActionsData", new EarTrumpetAction[] { });
                Actions = new ObservableCollection<EarTrumpetAction>(data);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Failure loading settings: {ex}");
                Actions = new ObservableCollection<EarTrumpetAction>();
            }

            foreach (var a in Actions)
            {
                a.Loaded();
                foreach (var t in a.Triggers)
                {
                    if (t is EventTrigger)
                    {
                        ((EventTrigger)t).RaiseEvent(EventTriggerType.EarTrumpet_Startup);
                    }
                }
            }
        }

        private void OnShuttingDown()
        {
            foreach (var a in Actions)
            {
                foreach (var t in a.Triggers)
                {
                    if (t is EventTrigger)
                    {
                        ((EventTrigger)t).RaiseEvent(EventTriggerType.EarTrumpet_Shutdown);
                    }
                }
            }
        }
    }
}
