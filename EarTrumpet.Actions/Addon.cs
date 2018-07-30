using EarTrumpet.DataModel.Storage;
using EarTrumpet.Extensibility;
using EarTrumpet.Extensibility.Shared;
using EarTrumpet.Extensions;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.Views;
using EarTrumpet_Actions.DataModel;
using EarTrumpet_Actions.DataModel.Processing;
using EarTrumpet_Actions.DataModel.Triggers;
using EarTrumpet_Actions.ViewModel;
using System.ComponentModel.Composition;
using System.Linq;

namespace EarTrumpet_Actions
{
    [Export(typeof(IAddonLifecycle))]
    public class Addon : IAddonLifecycle
    {
        public static Addon Current { get; private set; }
        public static string Namespace => "EarTrumpet-Actions";
        public LocalVariablesContainer LocalVariables { get; private set; }

        public EarTrumpetAction[] Actions
        {
            get => _actions;
            set
            {
                _settings.Set(c_actionsSettingKey, value);
                LoadAndRegister();
            }
        }

        private readonly string c_actionsSettingKey = "ActionsData";
        private EarTrumpetAction[] _actions = new EarTrumpetAction[] { };
        private SettingsWindow _openSettingsWindow;
        private ISettingsBag _settings = StorageFactory.GetSettings(Namespace);
        private TriggerManager _triggerManager = new TriggerManager();

        public void OnApplicationLifecycleEvent(ApplicationLifecycleEvent evt)
        {
            if (evt == ApplicationLifecycleEvent.Startup2)
            {
                Current = this;
                LocalVariables = new LocalVariablesContainer(_settings);

                LoadAndRegister();

                _triggerManager.Triggered += OnTriggered;
                _triggerManager.OnEvent(ApplicationLifecycleEvent.Startup);
            }
            else if (evt == ApplicationLifecycleEvent.Shutdown)
            {
                _triggerManager.OnEvent(ApplicationLifecycleEvent.Shutdown);
            }
        }

        private void LoadAndRegister()
        {
            _triggerManager.Clear();
            _actions = _settings.Get(c_actionsSettingKey, new EarTrumpetAction[] { });
            _actions.SelectMany(a => a.Triggers).ToList().ForEach(t => _triggerManager.Register(t));
        }

        public void OpenSettingsWindow()
        {
            if (_openSettingsWindow != null)
            {
                _openSettingsWindow.RaiseWindow();
            }
            else
            {
                ResourceLoader.Load(Namespace);

                var viewModel = new ActionsEditorViewModel(Actions);
                _openSettingsWindow = new SettingsWindow { DataContext = viewModel };
                _openSettingsWindow.Closing += (_, __) =>
                {
                    Actions = viewModel.Actions.Select(a => a.GetAction()).ToArray();
                    _openSettingsWindow = null;
                };
                _openSettingsWindow.Show();
                WindowAnimationLibrary.BeginWindowEntranceAnimation(_openSettingsWindow, () => { });
            }
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