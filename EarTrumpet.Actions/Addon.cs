using EarTrumpet.Extensibility;
using EarTrumpet.UI.Views;
using EarTrumpet_Actions.DataModel;
using EarTrumpet_Actions.ViewModel;
using System;
using System.ComponentModel.Composition;
using System.Windows;

namespace EarTrumpet_Actions
{
    [Export(typeof(IAddonLifecycle))]
    public class Addon : IAddonLifecycle
    {
        public static Addon Current { get; private set; }

        public ActionsEditorViewModel ViewModel { get; private set; }
        public ActionsManager Manager { get; private set; }
        public ProcessWatcher ProcessWatcher { get; private set; }

        public void OnApplicationLifecycleEvent(ApplicationLifecycleEvent evt)
        {
            if (evt == ApplicationLifecycleEvent.Startup)
            {
                Current = this;
                Manager = new ActionsManager();
                ProcessWatcher = new ProcessWatcher();
                Manager.OnStartup();

                ViewModel = new ActionsEditorViewModel();
                ViewModel.RequestApplyChanges += ViewModel_RequestApplyChanges;

                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
                {
                    Source = new Uri("/EarTrumpet-Actions;component/AddonResources.xaml", UriKind.RelativeOrAbsolute)
                });
            }
            else if (evt == ApplicationLifecycleEvent.Shutdown)
            {
                Manager.OnShuttingDown();
            }
        }

        private void ViewModel_RequestApplyChanges(EarTrumpetAction[] newActions)
        {
            Manager.Apply(newActions);
        }

        public void OpenSettingsWindow()
        {
            SettingsWindow.ActivateSingleInstance(ViewModel);
        }
    }
}