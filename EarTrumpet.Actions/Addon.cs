using EarTrumpet.Extensibility;
using EarTrumpet.Extensibility.Shared;
using EarTrumpet.Extensions;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.Services;
using EarTrumpet.UI.ViewModels;
using EarTrumpet.UI.Views;
using EarTrumpet_Actions.DataModel;
using EarTrumpet_Actions.ViewModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;

namespace EarTrumpet_Actions
{
    [Export(typeof(IAddonLifecycle))]
    public class Addon : IAddonLifecycle
    {
        public static Addon Current { get; private set; }
        public ActionsManager Manager { get; private set; }
        public ProcessWatcher ProcessWatcher { get; private set; }

        private SettingsWindow _openSettingsWindow;

        public void OnApplicationLifecycleEvent(ApplicationLifecycleEvent evt)
        {
            if (evt == ApplicationLifecycleEvent.Startup2)
            {
                Current = this;
                Manager = new ActionsManager();
                ProcessWatcher = new ProcessWatcher();
                Manager.OnStartup();
            }
            else if (evt == ApplicationLifecycleEvent.Shutdown)
            {
                Manager.OnShuttingDown();
            }
        }

        public void OpenSettingsWindow()
        {
            if (_openSettingsWindow != null)
            {
                _openSettingsWindow.RaiseWindow();
            }
            else
            {
                ResourceLoader.Load("EarTrumpet-Actions");

                var viewModel = new ActionsEditorViewModel();
                viewModel.OpenDialog = new RelayCommand<object>((dialogVm) =>
                {
                    var win = new DialogWindow
                    {
                        Owner = _openSettingsWindow,
                        DataContext = dialogVm
                    };

                    // HACK: rework this somehow so this work is scripted by the VM
                    if (dialogVm is HotkeySelectViewModel)
                    {
                        win.PreviewKeyDown += ((HotkeySelectViewModel)dialogVm).Window_PreviewKeyDown;
                        ((HotkeySelectViewModel)dialogVm).Save = new RelayCommand(() => win.Close());

                        HotkeyManager.Current.Pause();
                    }
                    if (dialogVm is AddNewPartViewModel)
                    {
                        ((AddNewPartViewModel)dialogVm).Close += () => win.Close();
                    }
                    
                    win.ShowDialog();

                    if (dialogVm is HotkeySelectViewModel)
                    {
                        HotkeyManager.Current.Resume();
                    }
                });

                _openSettingsWindow = new SettingsWindow();
                _openSettingsWindow.DataContext = viewModel;
                _openSettingsWindow.Closing += (_, __) =>
                {
                    Manager.Apply(viewModel.Actions.Select(a => a.GetAction()).ToArray());

                    _openSettingsWindow = null;
                };
                _openSettingsWindow.Show();
                WindowAnimationLibrary.BeginWindowEntranceAnimation(_openSettingsWindow, () => { });
            }
        }
    }
}